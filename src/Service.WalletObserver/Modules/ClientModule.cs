using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.Balances.Client;
using Service.IndexPrices.Client;
using Service.Liquidity.Converter.Client;

namespace Service.WalletObserver.Modules
{
    public class ClientModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.Settings.MyNoSqlReaderHostPort, Program.LogFactory);
            
            builder.RegisterBalancesClientsWithoutCache(Program.Settings.BalancesGrpcServiceUrl);
            builder.RegisterIndexPricesClient(myNoSqlClient);
            builder.RegisterLiquidityConverterManagerClient(Program.Settings.LiquidityConverterGrpcServiceUrl);
        }
    }
}