using System;
using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Domain.Models
{
    [DataContract]
    public class ApiKeyVolume
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string ApiKeyHash { get; set; }
        [DataMember(Order = 3)] public string Asset { get; set; }
        [DataMember(Order = 4)] public double Volume { get; set; }
        [DataMember(Order = 5)] public DateTime LastUpdateTime { get; set; }
    }
}