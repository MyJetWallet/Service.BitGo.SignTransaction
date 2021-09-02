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

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Service.BitGo.SignTransaction.Services
{
    public class SessionUnlockService : ISessionUnlockService
    {
        private readonly ILogger<SessionUnlockService> _logger;
        private readonly IPublisher<SignalBitGoSessionStateUpdate> _sessionPublisher;
        private readonly SymmetricEncryptionService _encryptionService;
        private readonly IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> _myNoSqlServerUserDataReader;

        private readonly BitGoClient _bitGoClient;

        public SessionUnlockService(ILogger<SessionUnlockService> logger,
            IPublisher<SignalBitGoSessionStateUpdate> sessionPublisher,
            IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> myNoSqlServerUserDataReader,
            SymmetricEncryptionService encryptionService)
        {
            _logger = logger;
            _sessionPublisher = sessionPublisher;
            _myNoSqlServerUserDataReader = myNoSqlServerUserDataReader;
            _encryptionService = encryptionService;

            _bitGoClient = new BitGoClient(null, Program.Settings.BitgoExpressUrl);
            _bitGoClient.ThrowThenErrorResponse = false;
        }

        public async Task<UnlockSessionResponse> UnlockSessionAsync(UnlockSessionRequest request)
        {
            _logger.LogInformation($"Session unlock request from: {request.UpdatedBy}");
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
                _bitGoClient.SetAccessToken(apiKey);

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