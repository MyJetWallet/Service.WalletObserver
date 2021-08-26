using System;
using System.Collections.Generic;
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
            var balanceSnapshot = await _internalWalletStorage.GetWalletsSnapshot();

            foreach (var walletName in balanceSnapshot.Select(e => e.WalletName).Distinct())
            {
                var walletId = balanceSnapshot.FirstOrDefault(e => e.WalletName == walletName)?.WalletId;
                
                if (string.IsNullOrWhiteSpace(walletId))
                    continue;

                var actualBalances = await _internalWalletObserverMath.GetInternalWalletBalanceCollection(walletId);
                var newBalances = new List<InternalWalletBalance>();
                
                foreach (var actualBalance in actualBalances)
                {
                    var lastBalance = balanceSnapshot.FirstOrDefault(e =>
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
                    newBalances.Add(newBalance);
                }
                await _internalWalletStorage.SaveWallet(newBalances);
            }
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}