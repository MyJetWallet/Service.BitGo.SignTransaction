using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class SpendingLimit
    {
        [DataMember(Order = 1)] public string AssetId { get; set; }
        [DataMember(Order = 2)] public string BitgoCoin { get; set; }
        [DataMember(Order = 3)] public double TransactionLimit { get; set; }
        [DataMember(Order = 4)] public double HourlyLimit { get; set; }
        [DataMember(Order = 5)] public double HourlySpent { get; set; }
        [DataMember(Order = 6)] public double DailyLimit { get; set; }
        [DataMember(Order = 7)] public double DailySpent { get; set; }
    }
}