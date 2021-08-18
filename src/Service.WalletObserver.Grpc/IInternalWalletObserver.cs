using System.ServiceModel;
using System.Threading.Tasks;
using Service.WalletObserver.Grpc.Models;

namespace Service.WalletObserver.Grpc
{
    [ServiceContract]
    public interface IInternalWalletObserver
    {
        [OperationContract]
        Task<AddNewWalletResponse> UpsertWalletAsync(AddNewWalletRequest request);
        
        [OperationContract]
        Task<GetWalletsResponse> GetWalletsAsync();
        
        [OperationContract]
        Task<RemoveWalletResponse> RemoveWalletAsync(RemoveWalletRequest request);
    }
}