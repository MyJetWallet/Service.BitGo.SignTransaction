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

                var result = await _bitGoClient.SendCoinsAsync(request.BitgoCoin, request.BitgoWalletId,
                    Program.Settings.GetPassphraseByWalletId(request.BitgoWalletId), request.SequenceId,
                    request.Amount, request.Address);

                if (!result.Success)
                {
                    _logger.LogError("Transfer Result: {jsonText}", JsonConvert.SerializeObject(result.Data));
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
                _logger.LogError(ex, "Transfer Request: {jsonText}. Error: {message}", JsonConvert.SerializeObject(request), ex.Message);
                throw;
            }
        }
    }
}
