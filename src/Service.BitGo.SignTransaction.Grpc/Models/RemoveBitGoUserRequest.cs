using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class RemoveBitGoUserRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string UserId { get; set; }
    }
}