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

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Service.BitGo.SignTransaction.Services
{
    public class SessionUnlockService : ISessionUnlockService
    {
        private readonly ILogger<SessionUnlockService> _logger;
        private readonly IPublisher<SignalBitGoSessionStateUpdate> _sessionPublisher;

        private readonly IBitGoClientService _bitGoClientService;

        public SessionUnlockService(ILogger<SessionUnlockService> logger,
            IPublisher<SignalBitGoSessionStateUpdate> sessionPublisher,
            IBitGoClientService bitGoClientService)
        {
            _logger = logger;
            _sessionPublisher = sessionPublisher;
            _bitGoClientService = bitGoClientService;
        }

        public async Task<UnlockSessionResponse> UnlockSessionAsync(UnlockSessionRequest request)
        {
            _logger.LogInformation($"Session unlock request from: {request.UpdatedBy}");
            try
            {
                var client = _bitGoClientService.GetByUser(request.BrokerId, BitGoUserNoSqlEntity.TechSignerId, request.CoinId);
                if (client == null)
                {
                    throw new Exception($"Tech account is not configured, id = {BitGoUserNoSqlEntity.TechSignerId}, coin = {request.CoinId}");
                }

                var result = await client.UnlockSessionAsync(request.Otp, request.Duration);
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