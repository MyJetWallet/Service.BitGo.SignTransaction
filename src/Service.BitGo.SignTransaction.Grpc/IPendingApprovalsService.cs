using System.ServiceModel;
using System.Threading.Tasks;
using MyJetWallet.BitGo.Models.PendingApproval;
using Service.BitGo.SignTransaction.Grpc.Models;

namespace Service.BitGo.SignTransaction.Grpc
{
    [ServiceContract]
    public interface IPendingApprovalsService
    {
        [OperationContract]
        Task<PendingApprovalInfo> GetPendingApprovalDetails(GetPendingApprovalRequest request);

        [OperationContract]
        Task<UpdatePendingApprovalResponse> UpdatePendingApproval(UpdatePendingApprovalRequest request);
    }
}