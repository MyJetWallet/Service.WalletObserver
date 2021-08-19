using Prometheus;
using Service.WalletObserver.Domain.Models;

namespace Service.WalletObserver.Services
{
    public class InternalWalletObserverMetrics
    {
        private static readonly Gauge InternalWalletBalanceUsd = Metrics
            .CreateGauge("jetwallet-internalwallet-balance-usd",
                "Usd balance by asset and wallet.",
                new GaugeConfiguration { LabelNames = new[] { "asset", "wallet"} });
        
        private static readonly Gauge InternalWalletBalance = Metrics
            .CreateGauge("jetwallet-internalwallet-balance",
                "Balance by asset and wallet.",
                new GaugeConfiguration { LabelNames = new[] { "asset", "wallet"} });
        
        private static readonly Gauge InternalWalletBalanceLimit = Metrics
            .CreateGauge("jetwallet-internalwallet-balance-limit",
                "Balance limit by asset and wallet.",
                new GaugeConfiguration { LabelNames = new[] { "asset", "wallet"} });
        
        private static readonly Gauge InternalWalletBalanceAlert = Metrics
            .CreateGauge("jetwallet-internalwallet-balance-alert",
                "Balance alert by asset and wallet.",
                new GaugeConfiguration { LabelNames = new[] { "asset", "wallet"} });

        public void SetMetrics(InternalWalletBalance balance)
        {
            InternalWalletBalanceUsd
                .WithLabels(balance.Asset, balance.WalletName)
                .Set((double) balance.UsdVolume);
            
            InternalWalletBalance
                .WithLabels(balance.Asset, balance.WalletName)
                .Set((double) balance.Volume);
            
            InternalWalletBalanceLimit
                .WithLabels(balance.Asset, balance.WalletName)
                .Set((double) balance.MinBalanceInUsd);

            var valueForAlert = balance.UsdVolume < balance.MinBalanceInUsd
                ? balance.UsdVolume
                : 0;
            
            InternalWalletBalanceAlert
                .WithLabels(balance.Asset, balance.WalletName)
                .Set((double) valueForAlert);
        }
    }
}