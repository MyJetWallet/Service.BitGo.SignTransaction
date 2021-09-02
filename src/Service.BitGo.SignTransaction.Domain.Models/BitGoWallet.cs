using System;
using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Domain.Models
{
    [DataContract]
    public class BitGoWallet
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public string Id { get; set; }
        [DataMember(Order = 3)] public string CoinId { get; set; }
        [DataMember(Order = 4)] public DateTime RegisterDate { get; set; }
        [DataMember(Order = 5)] public string ApiKey { get; set; }
        [DataMember(Order = 6)] public DateTime UpdatedDate { get; set; }
        [DataMember(Order = 7)] public string UpdatedBy { get; set; }
    }
}