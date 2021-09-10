using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class GetBitGoUserRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string UserId { get; set; }
        [DataMember(Order = 3)] public string CoinId { get; set; }
    }
}