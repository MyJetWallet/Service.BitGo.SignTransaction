using System;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using Newtonsoft.Json;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Grpc;
using Service.BitGo.SignTransaction.Grpc.Models;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Service.BitGo.SignTransaction.Services
{
    public class SessionUnlockService : ISessionUnlockService
    {
        private readonly ILogger<SessionUnlockService> _logger;
        private readonly IBitGoClient _bitGoClient;
        private readonly IPublisher<SignalBitGoSessionStateUpdate> _sessionPublisher;

        public SessionUnlockService(ILogger<SessionUnlockService> logger, IBitGoClient bitGoClient,
            IPublisher<SignalBitGoSessionStateUpdate> sessionPublisher)
        {
            _logger = logger;
            _bitGoClient = bitGoClient;
            _sessionPublisher = sessionPublisher;
        }

        public async Task<UnlockSessionResponse> UnlockSessionAsync(UnlockSessionRequest request)
        {
            _logger.LogInformation($"Session unlock request from: {request.UpdatedBy}");
            try
            {
                var result = await _bitGoClient.UnlockSessionAsync(request.Otp, request.Duration);
                if (!result.Success)
                {
                    _logger.LogInformation($"Unable to unlock session: {JsonConvert.SerializeObject(result.Error)}");
                    return new UnlockSessionResponse()
                    {
                        Error = result.Error
                    };
                }
                _logger.LogInformation("Session unlocked");
                
                await _sessionPublisher.PublishAsync(new SignalBitGoSessionStateUpdate()
                {
                    State = BitGoSessionState.Unlocked,
                    UpdatedBy = request.UpdatedBy,
                    UpdatedDate = DateTime.Now
                });
                return new UnlockSessionResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session unlock request error: {message}", ex.Message);
                throw;
            }
        }
    }
}