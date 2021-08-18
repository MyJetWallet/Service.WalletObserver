using System.ServiceModel;
using System.Threading.Tasks;
using Service.WalletObserver.Grpc.Models;

namespace Service.WalletObserver.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}