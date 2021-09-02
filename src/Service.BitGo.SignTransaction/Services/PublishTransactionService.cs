using System;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;
using Service.BitGo.SignTransaction.Grpc;
using Service.BitGo.SignTransaction.Grpc.Models;

// ReSharper disable InconsistentLogPropertyNaming

namespace Service.BitGo.SignTransaction.Services
{
    public class PublishTransactionService : IPublishTransactionService
    {
        private readonly ILogger<PublishTransactionService> _logger;
        private readonly IPublisher<SignalBitGoSessionStateUpdate> _sessionPublisher;
        private readonly IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> _myNoSqlServerUserDataReader;
        private readonly IMyNoSqlServerDataReader<BitGoWalletNoSqlEntity> _myNoSqlServerWalletDataReader;
        private readonly SymmetricEncryptionService _encryptionService;

        private readonly BitGoClient _bitGoClient;

        public PublishTransactionService(ILogger<PublishTransactionService> logger,
            IPublisher<SignalBitGoSessionStateUpdate> sessionPublisher,
            IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> myNoSqlServerUserDataReader,
            IMyNoSqlServerDataReader<BitGoWalletNoSqlEntity> myNoSqlServerWalletDataReader,
            SymmetricEncryptionService encryptionService)
        {
            _logger = logger;
            _sessionPublisher = sessionPublisher;
            _myNoSqlServerUserDataReader = myNoSqlServerUserDataReader;
            _myNoSqlServerWalletDataReader = myNoSqlServerWalletDataReader;
            _encryptionService = encryptionService;

            _bitGoClient = new BitGoClient(null, Program.Settings.BitgoApiUrl);
            _bitGoClient.ThrowThenErrorResponse = false;
        }

        public async Task<SendTransactionResponse> SignAndSendTransactionAsync(SendTransactionRequest request)
        {
            _logger.LogInformation("Transfer Request: {jsonText}", JsonConvert.SerializeObject(request));

            try
            {
                var bitGoUser = _myNoSqlServerUserDataReader.Get(
                    BitGoUserNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                    BitGoUserNoSqlEntity.GenerateRowKey(BitGoUserNoSqlEntity.TechSignerId));
                if (string.IsNullOrEmpty(bitGoUser?.User?.ApiKey))
                {
                    _logger.LogError("Tech account is not configured, id = {techSignerName}",
                        BitGoUserNoSqlEntity.TechSignerId);
                    throw new Exception($"Tech account is not configured, id = {BitGoUserNoSqlEntity.TechSignerId}");
                }

                var apiKey = _encryptionService.Decrypt(bitGoUser.User.ApiKey);

                var wallet = _myNoSqlServerWalletDataReader.Get(
                    BitGoWalletNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                    BitGoWalletNoSqlEntity.GenerateRowKey(request.BitgoWalletId));
                if (string.IsNullOrEmpty(wallet?.Wallet?.ApiKey))
                {
                    _logger.LogError("Cannot find pass phase for wallet {bitgoWalletIdText}", request.BitgoWalletId);
                    throw new Exception($"Cannot find pass phase for wallet {request.BitgoWalletId}");
                }

                var walletPass = _encryptionService.Decrypt(wallet.Wallet.ApiKey);

                _bitGoClient.SetAccessToken(apiKey);
                var result = await _bitGoClient.SendCoinsAsync(request.BitgoCoin, request.BitgoWalletId,
                    walletPass, request.SequenceId,
                    request.Amount, request.Address);

                if (!result.Success)
                {
                    switch (result.Error.Code)
                    {
                        case "DuplicateSequenceIdError":
                        {
                            var transaction = await _bitGoClient.GetTransferBySequenceIdAsync(request.BitgoCoin,
                                request.BitgoWalletId, request.SequenceId);

                            if (!transaction.Success || transaction.Data == null)
                            {
                                _logger.LogError("Transfer is Duplicate, but cannot found transaction: {jsonText}",
                                    JsonConvert.SerializeObject(transaction.Error));

                                return new SendTransactionResponse()
                                {
                                    Error = result.Error
                                };
                            }

                            _logger.LogInformation("Transfer is Duplicate, Result: {jsonText}",
                                JsonConvert.SerializeObject(transaction.Data));

                            return new SendTransactionResponse()
                            {
                                DuplicateTransaction = transaction.Data
                            };
                        }
                        case "needs unlock":
                            await _sessionPublisher.PublishAsync(new SignalBitGoSessionStateUpdate()
                            {
                                State = BitGoSessionState.Locked
                            });
                            return new SendTransactionResponse()
                            {
                                Error =
                                {
                                    Code = "needs unlock",
                                    ErrorMessage = "Session is locked"
                                }
                            };
                    }
                }

                if (!result.Success)
                {
                    _logger.LogError("Transfer Result: {jsonText}", JsonConvert.SerializeObject(result.Error));
                    return new SendTransactionResponse()
                    {
                        Error = result.Error
                    };
                }

                _logger.LogInformation("Transfer Result: {jsonText}", JsonConvert.SerializeObject(result.Data));
                return new SendTransactionResponse()
                {
                    Result = result.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transfer Request ERROR: {jsonText}. Error: {message}",
                    JsonConvert.SerializeObject(request), ex.Message);
                throw;
            }
        }
    }
}