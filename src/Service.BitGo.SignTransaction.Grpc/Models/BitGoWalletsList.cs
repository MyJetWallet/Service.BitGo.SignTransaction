using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.BitGo.SignTransaction.Domain.Models;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class BitGoWalletsList
    {
        [DataMember(Order = 1)] public List<BitGoWallet> Wallets { get; set; }
    }
}