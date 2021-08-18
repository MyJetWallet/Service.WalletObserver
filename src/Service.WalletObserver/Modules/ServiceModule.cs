using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.NoSql;
using Service.WalletObserver.Domain;
using Service.WalletObserver.Domain.Models;

namespace Service.WalletObserver.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<InternalWalletNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), InternalWalletNoSql.TableName);
            
            builder
                .RegisterType<InternalWalletStorage>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}