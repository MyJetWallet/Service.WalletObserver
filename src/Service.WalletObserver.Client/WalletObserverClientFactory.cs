using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.WalletObserver.Grpc;

namespace Service.WalletObserver.Client
{
    [UsedImplicitly]
    public class WalletObserverClientFactory: MyGrpcClientFactory
    {
        public WalletObserverClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IInternalWalletObserver GetHelloService() => CreateGrpcService<IInternalWalletObserver>();
    }
}
