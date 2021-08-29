using System.Runtime.Serialization;
using MyJetWallet.BitGo.Models;
using MyJetWallet.BitGo.Models.Express;
using MyJetWallet.BitGo.Models.Transfer;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class UnlockSessionResponse
    { 
        [DataMember(Order = 1)] public Error Error { get; set; }
    }
}