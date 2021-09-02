using System.ServiceModel;
using System.Threading.Tasks;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Grpc.Models;

namespace Service.BitGo.SignTransaction.Grpc
{
    [ServiceContract]
    public interface IBitGoWalletsService
    {
        [OperationContract]
        Task<BitGoWalletsList> GetBitGoWalletsList();

        [OperationContract]
        Task<BitGoWallet> GetBitGoWallet(GetBitGoWalletRequest request);

        [OperationContract]
        Task AddBitGoWallet(BitGoWallet wallet);

        [OperationContract]
        Task UpdateBitGoWallet(BitGoWallet wallet);

        [OperationContract]
        Task RemoveBitGoWallet(RemoveBitGoWalletRequest request);
    }
}