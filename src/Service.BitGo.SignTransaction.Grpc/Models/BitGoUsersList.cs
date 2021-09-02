using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.BitGo.SignTransaction.Domain.Models;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class BitGoUsersList
    {
        [DataMember(Order = 1)] public List<BitGoUser> Users { get; set; }
    }
}