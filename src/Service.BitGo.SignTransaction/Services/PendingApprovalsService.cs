using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using MyJetWallet.BitGo.Models.PendingApproval;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;
using Service.BitGo.SignTransaction.Grpc;
using Service.BitGo.SignTransaction.Grpc.Models;
using Service.BitGo.SignTransaction.Utils;

// ReSharper disable InconsistentLogPropertyNaming

namespace Service.BitGo.SignTransaction.Services
{
    public class PendingApprovalsService : IPendingApprovalsService
    {
        private readonly ILogger<PendingApprovalsService> _logger;
        private readonly IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> _myNoSqlServerUserDataReader;
        private readonly SymmetricEncryptionService _encryptionService;

        private readonly BitGoClient _bitGoClient;

        public PendingApprovalsService(ILogger<PendingApprovalsService> logger,
            IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> myNoSqlServerUserDataReader,
            SymmetricEncryptionService encryptionService)
        {
            _logger = logger;
            _myNoSqlServerUserDataReader = myNoSqlServerUserDataReader;
            _encryptionService = encryptionService;

            _bitGoClient = new BitGoClient(null, Program.Settings.BitgoExpressUrl);
            _bitGoClient.ThrowThenErrorResponse = false;
        }

        public async Task<PendingApprovalInfo> GetPendingApprovalDetails(GetPendingApprovalRequest request)
        {
            _logger.LogInformation("Get Pending Approval Details: {details}", JsonConvert.SerializeObject(request));

            var bitGoUser = _myNoSqlServerUserDataReader.Get(
                BitGoUserNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                BitGoUserNoSqlEntity.GenerateRowKey(BitGoUserNoSqlEntity.TechSignerId, request.CoinId));
            if (string.IsNullOrEmpty(bitGoUser?.User?.ApiKey))
            {
                _logger.LogError("Tech account is not configured, id = {techSignerName}",
                    BitGoUserNoSqlEntity.TechSignerId);
                return null;
            }

            var apiKey = _encryptionService.Decrypt(bitGoUser.User.ApiKey);
            _bitGoClient.SetAccessToken(apiKey);

            var approvalResp =
                await _bitGoClient.GetPendingApprovalAsync(request.PendingApprovalId);
            if (!approvalResp.Success)
            {
                _logger.LogInformation("Unable to get Pending Approval Details: {error}",
                    JsonConvert.SerializeObject(approvalResp.Error));
            }

            return approvalResp.Data;
        }

        public async Task<UpdatePendingApprovalResponse> UpdatePendingApproval(UpdatePendingApprovalRequest request)
        {
            _logger.LogInformation("Get Pending Approval Details: {details}",
                JsonConvert.SerializeObject(request,
                    new ApiKeyHiddenJsonConverter(typeof(UpdatePendingApprovalRequest))));

            var bitGoUser = _myNoSqlServerUserDataReader.Get(
                BitGoUserNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                BitGoUserNoSqlEntity.GenerateRowKey(request.UserId, request.CoinId));
            if (string.IsNullOrEmpty(bitGoUser?.User?.ApiKey))
            {
                _logger.LogError("BitGo user is not configured, id = {userId}",
                    request.UserId);
                return null;
            }

            var apiKey = _encryptionService.Decrypt(bitGoUser.User.ApiKey);
            _bitGoClient.SetAccessToken(apiKey);

            var approvalResp =
                await _bitGoClient.UpdatePendingApprovalAsync(request.PendingApprovalId, request.Otp,
                    request.State.ToString().ToLower());
            if (!approvalResp.Success)
            {
                _logger.LogInformation("Unable to update Pending Approval: {error}",
                    JsonConvert.SerializeObject(approvalResp.Error));
                return new UpdatePendingApprovalResponse
                {
                    Error = approvalResp.Error
                };
            }

            return new UpdatePendingApprovalResponse
            {
                PendingApprovalInfo = approvalResp.Data
            };
        }
    }
}