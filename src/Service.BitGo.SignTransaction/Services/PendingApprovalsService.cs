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

        private readonly IBitGoClientService _bitGoClientService;

        public PendingApprovalsService(ILogger<PendingApprovalsService> logger,
            IBitGoClientService bitGoClientService)
        {
            _logger = logger;
            _bitGoClientService = bitGoClientService;
        }

        public async Task<PendingApprovalInfo> GetPendingApprovalDetails(GetPendingApprovalRequest request)
        {
            _logger.LogInformation("Get Pending Approval Details: {details}", JsonConvert.SerializeObject(request));

            var client = _bitGoClientService.GetByUser(request.BrokerId, BitGoUserNoSqlEntity.TechSignerId, request.CoinId);
            if (client == null) return null;

            var approvalResp =
                await client.GetPendingApprovalAsync(request.PendingApprovalId);
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

            var client = _bitGoClientService.GetByUser(request.BrokerId, request.UserId, request.CoinId);
            if (client == null) return null;
            
            var approvalResp =
                await client.UpdatePendingApprovalAsync(request.PendingApprovalId, request.Otp,
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