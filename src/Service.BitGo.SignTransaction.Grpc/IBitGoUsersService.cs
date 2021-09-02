using System.ServiceModel;
using System.Threading.Tasks;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Grpc.Models;

namespace Service.BitGo.SignTransaction.Grpc
{
    [ServiceContract]
    public interface IBitGoUsersService
    {
        [OperationContract]
        Task<BitGoUsersList> GetBitGoUsersList(GetBitGoUsersRequest request);

        [OperationContract]
        Task<BitGoUser> GetBitGoUser(GetBitGoUserRequest request);

        [OperationContract]
        Task AddBitGoUser(BitGoUser user);

        [OperationContract]
        Task UpdateBitGoUser(BitGoUser user);

        [OperationContract]
        Task RemoveBitGoUser(RemoveBitGoUserRequest request);
    }
}