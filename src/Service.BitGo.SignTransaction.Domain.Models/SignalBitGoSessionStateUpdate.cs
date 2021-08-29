using System;
using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Domain.Models
{
    public class SignalBitGoSessionStateUpdate
    {
        public const string ServiceBusTopicName = "bitgo-session-update-signal";

        [DataMember(Order = 1)] public BitGoSessionState State { get; set; }    
        [DataMember(Order = 2)] public string UpdatedBy { get; set; }    
        [DataMember(Order = 3)] public DateTime UpdatedDate { get; set; }    
    }
}