using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prometheus;
using Service.WalletObserver.Domain.Models;

namespace Service.WalletObserver.Services
{
    public class InternalWalletObserverMetrics
    {
        private readonly ILogger<InternalWalletObserverMetrics> _logger;
        
        private static readonly Gauge InternalWalletBalanceUsd = Metrics
            .CreateGauge("jetwallet_internalwallet_balance_usd",
                "Usd balance by asset and wallet.",
                new GaugeConfiguration { LabelNames = new[] { "asset", "wallet"} });
        
        private static readonly Gauge InternalWalletBalance = Metrics
            .CreateGauge("jetwallet_internalwallet_balance",
                "Balance by asset and wallet.",
                new GaugeConfiguration { LabelNames = new[] { "asset", "wallet"} });
        
        private static readonly Gauge InternalWalletBalanceLimit = Metrics
            .CreateGauge("jetwallet_internalwallet_balance_limit",
                "Balance limit by asset and wallet.",
                new GaugeConfiguration { LabelNames = new[] { "asset", "wallet"} });
        
        private static readonly Gauge InternalWalletBalanceAlert = Metrics
            .CreateGauge("jetwallet_internalwallet_balance_alert",
                "Balance alert by asset and wallet.",
                new GaugeConfiguration { LabelNames = new[] { "asset", "wallet"} });

        public InternalWalletObserverMetrics(ILogger<InternalWalletObserverMetrics> logger)
        {
            _logger = logger;
        }

        public void SetMetrics(InternalWalletBalance balance)
        {
            try
            {
                InternalWalletBalanceUsd
                    .WithLabels(balance.Asset, balance.WalletName)
                    .Set(Convert.ToDouble(balance.UsdVolume));

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
            catch (Exception ex)
            {
                _logger.LogDebug($"InternalWalletObserverMetrics throw ex: {JsonConvert.SerializeObject(ex)}");
            }
        }
    }
}