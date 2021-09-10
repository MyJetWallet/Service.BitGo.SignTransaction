using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class UnlockSessionRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public int Duration { get; set; }
        [DataMember(Order = 3)] public string Otp { get; set; }
        [DataMember(Order = 4)] public string UpdatedBy { get; set; }
        [DataMember(Order = 5)] public string CoinId { get; set; }
    }
}