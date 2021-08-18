using Autofac;
using Service.Balances.Client;

namespace Service.WalletObserver.Modules
{
    public class ClientModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterBalancesClientsWithoutCache(Program.Settings.BalancesGrpcServiceUrl);
        }
    }
}