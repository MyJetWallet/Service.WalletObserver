using Autofac;
using Service.WalletObserver.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.WalletObserver.Client
{
    public static class AutofacHelper
    {
        public static void RegisterWalletObserverClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new WalletObserverClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IInternalWalletObserver>().SingleInstance();
        }
    }
}
