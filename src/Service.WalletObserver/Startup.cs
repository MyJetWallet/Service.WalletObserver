using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.GrpcMetrics;
using MyJetWallet.Sdk.GrpcSchema;
using MyJetWallet.Sdk.Service;
using Prometheus;
using ProtoBuf.Grpc.Server;
using Service.WalletObserver.Grpc;
using Service.WalletObserver.Modules;
using Service.WalletObserver.Services;
using Service.WalletObserver.Services.Grpc;
using SimpleTrading.BaseMetrics;
using SimpleTrading.ServiceStatusReporterConnector;

namespace Service.WalletObserver
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureJetWallet<ApplicationLifetimeManager>(Program.Settings.ZipkinUrl);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ConfigureJetWallet(env, endpoints =>
            {
                endpoints.MapGrpcSchema<InternalWalletObserverServiceService, IInternalWalletObserverService>();
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.ConfigureJetWallet();
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
            builder.RegisterModule<ClientModule>();
        }
    }
    
}
