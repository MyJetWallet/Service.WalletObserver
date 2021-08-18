using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.Balances.Client;
using Service.IndexPrices.Client;

namespace Service.WalletObserver.Modules
{
    public class ClientModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            
            builder.RegisterBalancesClientsWithoutCache(Program.Settings.BalancesGrpcServiceUrl);
            builder.RegisterIndexPricesClient(myNoSqlClient);
        }
    }
}