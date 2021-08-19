using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.WalletObserver.Domain.Models;
using Service.WalletObserver.Jobs;
using Service.WalletObserver.Services;

namespace Service.WalletObserver.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<InternalWalletNoSql>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), InternalWalletNoSql.TableName);
            
            builder
                .RegisterType<InternalWalletStorage>()
                .AsSelf()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            
            builder
                .RegisterType<InternalWalletObserverJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
            
            builder
                .RegisterType<InternalWalletObserverMath>()
                .AsSelf();
            
            builder
                .RegisterType<InternalWalletObserverMetrics>()
                .AsSelf()
                .SingleInstance();
        }
    }
}