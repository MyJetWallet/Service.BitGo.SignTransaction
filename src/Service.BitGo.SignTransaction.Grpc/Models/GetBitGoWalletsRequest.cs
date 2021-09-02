using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class GetBitGoWalletsRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
    }
}