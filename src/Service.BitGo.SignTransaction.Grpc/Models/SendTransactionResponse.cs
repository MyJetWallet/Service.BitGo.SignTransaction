using System.Runtime.Serialization;
using MyJetWallet.BitGo.Models;
using MyJetWallet.BitGo.Models.Express;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class SendTransactionResponse
    {
        [DataMember(Order = 1)] public SendCoinResult Result { get; set; }
        [DataMember(Order = 2)] public Error Error { get; set; }
    }
}