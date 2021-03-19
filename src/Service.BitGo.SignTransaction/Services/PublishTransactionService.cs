using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using Newtonsoft.Json;
using Service.BitGo.SignTransaction.Grpc;
using Service.BitGo.SignTransaction.Grpc.Models;

namespace Service.BitGo.SignTransaction.Services
{
    public class PublishTransactionService : IPublishTransactionService
    {
        private readonly ILogger<PublishTransactionService> _logger;
        private readonly IBitGoClient _bitGoClient;

        public PublishTransactionService(ILogger<PublishTransactionService> logger, IBitGoClient bitGoClient)
        {
            _logger = logger;
            _bitGoClient = bitGoClient;
        }

        public async Task<SendTransactionResponse> SignAndSendTransactionAsync(SendTransactionRequest request)
        {
            try
            {
                _logger.LogInformation("Transfer Request: {jsonText}", JsonConvert.SerializeObject(request));

                var pass = Program.Settings.GetPassphraseByWalletId(request.BitgoWalletId);

                if (string.IsNullOrEmpty(pass))
                {
                    _logger.LogError("Cannot find pass phase for wallet {bitgoWalletIdText}", request.BitgoWalletId);
                }

                var result = await _bitGoClient.SendCoinsAsync(request.BitgoCoin, request.BitgoWalletId,
                    pass, request.SequenceId,
                    request.Amount, request.Address);

                
                if (!result.Success && result.Error.Code == "DuplicateSequenceIdError")
                {
                    var transaction = await _bitGoClient.GetTransferBySequenceIdAsync(request.BitgoCoin, request.BitgoWalletId, request.SequenceId);
                    
                    if (!transaction.Success || transaction.Data == null)
                    {
                        _logger.LogError("Transfer is Duplicate, but cannot found transaction: {jsonText}", JsonConvert.SerializeObject(transaction.Error));

                        return new SendTransactionResponse()
                        {
                            Error = result.Error
                        };
                    }

                    _logger.LogInformation("Transfer is Duplicate, Result: {jsonText}", JsonConvert.SerializeObject(transaction.Data));

                    return new SendTransactionResponse()
                    {
                        DuplicateTransaction = transaction.Data
                    };
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
                _logger.LogError(ex, "Transfer Request ERROR: {jsonText}. Error: {message}", JsonConvert.SerializeObject(request), ex.Message);
                throw;
            }
        }
    }
}
