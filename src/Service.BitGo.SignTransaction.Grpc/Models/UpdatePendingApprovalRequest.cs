using System.Runtime.Serialization;
using Service.BitGo.SignTransaction.Domain.Models;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class UpdatePendingApprovalRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string UserId { get; set; }
        [DataMember(Order = 3)] public string Otp { get; set; }
        [DataMember(Order = 4)] public string PendingApprovalId { get; set; }
        [DataMember(Order = 5)] public PendingApprovalUpdatedState State { get; set; }
        [DataMember(Order = 6)] public string UpdatedBy { get; set; }
    }
}