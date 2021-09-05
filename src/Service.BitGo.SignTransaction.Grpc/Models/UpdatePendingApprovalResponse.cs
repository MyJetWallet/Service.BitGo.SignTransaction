using System.Runtime.Serialization;
using MyJetWallet.BitGo.Models;
using MyJetWallet.BitGo.Models.PendingApproval;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class UpdatePendingApprovalResponse
    {
        [DataMember(Order = 1)] public Error Error { get; set; }
        [DataMember(Order = 2)] public PendingApprovalInfo PendingApprovalInfo { get; set; }
    }
}