using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.WalletObserver.Domain.Models;
using Service.WalletObserver.Services;

namespace Service.WalletObserver.Jobs
{
    public class InternalWalletObserverJob : IStartable
    {
        private readonly ILogger<InternalWalletObserverJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly InternalWalletStorage _internalWalletStorage;
        private readonly InternalWalletObserverMath _internalWalletObserverMath;
        private readonly InternalWalletObserverMetrics _internalWalletObserverMetrics;

        public InternalWalletObserverJob(ILogger<InternalWalletObserverJob> logger,
            InternalWalletStorage internalWalletStorage,
            InternalWalletObserverMath internalWalletObserverMath,
            InternalWalletObserverMetrics internalWalletObserverMetrics)
        {
            _logger = logger;
            _internalWalletStorage = internalWalletStorage;
            _internalWalletObserverMath = internalWalletObserverMath;
            _internalWalletObserverMetrics = internalWalletObserverMetrics;
            _timer = new MyTaskTimer(nameof(InternalWalletObserverJob), TimeSpan.FromSeconds(Program.Settings.BalanceUpdateTimerInSeconds), _logger, DoTime);
        }

        private async Task DoTime()
        {
            await UpdateWalletBalances();
        }

        private async Task UpdateWalletBalances()
        {
            var walletInNoSql = await _internalWalletStorage.GetWalletsAsync();

            foreach (var walletName in walletInNoSql.Select(e => e.WalletName).Distinct())
            {
                var actualBalances = await _internalWalletObserverMath.GetInternalWalletBalanceCollection(walletName);

                foreach (var actualBalance in actualBalances)
                {
                    var lastBalance = walletInNoSql.FirstOrDefault(e =>
                        e.WalletName == walletName && e.Asset == actualBalance.Asset);

                    InternalWalletBalance newBalance;
                    
                    if (lastBalance != null)
                    {
                        newBalance = lastBalance;
                        newBalance.Volume = actualBalance.Volume;
                        newBalance.UsdVolume = actualBalance.UsdVolume;
                    }
                    else
                    {
                        newBalance = new InternalWalletBalance()
                        {
                            Asset = actualBalance.Asset,
                            WalletName = walletName,
                            UsdVolume = actualBalance.UsdVolume,
                            Volume = actualBalance.Volume,
                            MinBalanceInUsd = 0m
                        };
                    }
                    _internalWalletObserverMetrics.SetMetrics(newBalance);
                    await _internalWalletStorage.SaveWallet(newBalance);
                }
            }
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}