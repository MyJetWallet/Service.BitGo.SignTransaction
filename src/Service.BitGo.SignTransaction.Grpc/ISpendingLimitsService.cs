using System.ServiceModel;
using System.Threading.Tasks;
using Service.BitGo.SignTransaction.Grpc.Models;

namespace Service.BitGo.SignTransaction.Grpc
{
    [ServiceContract]
    public interface ISpendingLimitsService
    {
        [OperationContract]
        Task<GetBitGoWalletLimitsResponse> GetSpendingLimitsAsync(GetBitGoWalletLimitsRequest request);
    }
}