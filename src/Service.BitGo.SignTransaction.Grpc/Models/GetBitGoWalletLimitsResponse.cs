using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class GetBitGoWalletLimitsResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string Error { get; set; }
        [DataMember(Order = 3)] public SpendingLimit Limit { get; set; }
    }
}