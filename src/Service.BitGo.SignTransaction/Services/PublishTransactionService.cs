using System;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using MyJetWallet.BitGo.Settings.Services;
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
        private readonly IAssetMapper _assetMapper;
        private readonly IPublisher<SignalBitGoSessionStateUpdate> _sessionPublisher;
        private readonly IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> _myNoSqlServerUserDataReader;
        private readonly IMyNoSqlServerDataReader<BitGoWalletNoSqlEntity> _myNoSqlServerWalletDataReader;
        private readonly IMyNoSqlServerDataWriter<ApiKeyVolumeNoSqlEntity> _myNoSqlServerApiKeyDataWriter;
        private readonly SymmetricEncryptionService _encryptionService;

        private readonly BitGoClient _bitGoClient;

        public PublishTransactionService(ILogger<PublishTransactionService> logger,
            IAssetMapper assetMapper,
            IPublisher<SignalBitGoSessionStateUpdate> sessionPublisher,
            IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> myNoSqlServerUserDataReader,
            IMyNoSqlServerDataReader<BitGoWalletNoSqlEntity> myNoSqlServerWalletDataReader,
            IMyNoSqlServerDataWriter<ApiKeyVolumeNoSqlEntity> myNoSqlServerApiKeyDataWriter,
            SymmetricEncryptionService encryptionService)
        {
            _logger = logger;
            _assetMapper = assetMapper;
            _sessionPublisher = sessionPublisher;
            _myNoSqlServerUserDataReader = myNoSqlServerUserDataReader;
            _myNoSqlServerWalletDataReader = myNoSqlServerWalletDataReader;
            _myNoSqlServerApiKeyDataWriter = myNoSqlServerApiKeyDataWriter;
            _encryptionService = encryptionService;

            _bitGoClient = new BitGoClient(null, Program.Settings.BitgoExpressUrl);
            _bitGoClient.ThrowThenErrorResponse = false;
        }

        public async Task<SendTransactionResponse> SignAndSendTransactionAsync(SendTransactionRequest request)
        {
            _logger.LogInformation("Transfer Request: {jsonText}", JsonConvert.SerializeObject(request));

            try
            {
                var bitGoUser = _myNoSqlServerUserDataReader.Get(
                    BitGoUserNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                    BitGoUserNoSqlEntity.GenerateRowKey(BitGoUserNoSqlEntity.TechSignerId, request.BitgoCoin));

                if (bitGoUser == null)
                {
                    _myNoSqlServerUserDataReader.Get(
                        BitGoUserNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                        BitGoUserNoSqlEntity.GenerateRowKey(BitGoUserNoSqlEntity.TechSignerId,
                            BitGoUserNoSqlEntity.DefaultCoin));
                }

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

                await AddVolumeToApiKey(request.BrokerId, _encryptionService.GetSha256Hash(wallet.Wallet.ApiKey),
                    request.BitgoCoin,
                    _assetMapper.ConvertAmountFromBitgo(request.BitgoCoin, long.Parse(request.Amount)));

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

        private async Task AddVolumeToApiKey(string brokerId, string apiKey, string asset, double amount)
        {
            try
            {
                var existingEntity = await _myNoSqlServerApiKeyDataWriter.GetAsync(
                    ApiKeyVolumeNoSqlEntity.GeneratePartitionKey(brokerId),
                    ApiKeyVolumeNoSqlEntity.GenerateRowKey(apiKey, asset));

                if (existingEntity == null)
                {
                    existingEntity = ApiKeyVolumeNoSqlEntity.Create(new ApiKeyVolume
                    {
                        BrokerId = brokerId,
                        ApiKeyHash = apiKey,
                        Asset = asset,
                        Volume = amount,
                        LastUpdateTime = DateTime.Now
                    });
                }
                else
                {
                    existingEntity.Volume.Volume += amount;
                    existingEntity.Volume.LastUpdateTime = DateTime.Now;
                }

                await _myNoSqlServerApiKeyDataWriter.InsertOrReplaceAsync(existingEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unable to update accumulated volume for api key {apiKey}, asset {asset}, amount {amount}", apiKey,
                    asset, amount);
            }
        }
    }
}