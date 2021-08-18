using System.ServiceModel;
using System.Threading.Tasks;
using Service.WalletObserver.Grpc.Models;

namespace Service.WalletObserver.Grpc
{
    [ServiceContract]
    public interface IInternalWalletObserver
    {
        [OperationContract]
        Task<AddNewWalletResponse> AddNewWalletAsync(AddNewWalletRequest request);
        
        [OperationContract]
        Task<GetWalletsResponse> GetWalletsAsync();
        
        [OperationContract]
        Task<ChangeWalletMinBalanceResponse> ChangeWalletMinBalanceAsync(ChangeWalletMinBalanceRequest request);
        
        [OperationContract]
        Task<RemoveWalletResponse> RemoveWalletAsync(RemoveWalletRequest request);
    }
}