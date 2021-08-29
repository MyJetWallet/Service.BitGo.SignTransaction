using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    public class UnlockSessionRequest
    {
        [DataMember(Order = 1)] public int Duration { get; set; }
        [DataMember(Order = 2)] public string Otp { get; set; }
        [DataMember(Order = 3)] public string UpdatedBy { get; set; }
    }
}