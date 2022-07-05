using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Polly;
using Polly.Retry;
using Service.HighYieldEngine.Grpc;
using Service.Liquidity.Converter.Grpc;
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
        private readonly ILiquidityConverterSettingsManager _converterSettingsManager;
        private readonly IHighYieldEngineBackofficeService _highYieldService;
        private readonly AsyncRetryPolicy _rpcUnavailableRetryPolicy;

        public InternalWalletObserverJob(ILogger<InternalWalletObserverJob> logger,
            InternalWalletStorage internalWalletStorage,
            InternalWalletObserverMath internalWalletObserverMath,
            InternalWalletObserverMetrics internalWalletObserverMetrics,
            ILiquidityConverterSettingsManager converterSettingsManager,
            IHighYieldEngineBackofficeService highYieldService
        )
        {
            _logger = logger;
            _internalWalletStorage = internalWalletStorage;
            _internalWalletObserverMath = internalWalletObserverMath;
            _internalWalletObserverMetrics = internalWalletObserverMetrics;
            _converterSettingsManager = converterSettingsManager;
            _highYieldService = highYieldService;
            _timer = new MyTaskTimer(nameof(InternalWalletObserverJob),
                TimeSpan.FromSeconds(Program.Settings.BalanceUpdateTimerInSeconds), _logger, DoTime);
            _rpcUnavailableRetryPolicy = Policy
                .Handle<RpcException>(ex => ex.StatusCode == StatusCode.Unavailable)
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, retryCount, context) =>
                    {
                        logger.LogWarning(
                            $"Failed request {context.OperationKey}, retrying {retryCount}. {exception.Message} {exception.StackTrace}");
                    });
        }

        private async Task DoTime()
        {
            await UpdateWalletBalances();
        }

        private async Task UpdateWalletBalances()
        {
            var balanceSnapshot = await _internalWalletStorage.GetWalletsAsync();
            var newBalances = new List<InternalWalletBalance>();

            foreach (var walletName in balanceSnapshot.Select(e => e.WalletName).Distinct())
            {
                var wallet = balanceSnapshot.FirstOrDefault(e => e.WalletName == walletName);

                if (string.IsNullOrWhiteSpace(wallet?.WalletId))
                    continue;

                try
                {
                    var actualBalances =
                        await _internalWalletObserverMath.GetInternalWalletBalanceCollection(wallet.WalletId);

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
                            newBalance = new InternalWalletBalance
                            {
                                Asset = actualBalance.Asset,
                                WalletName = walletName,
                                WalletId = wallet.WalletId,
                                BrokerId = wallet.BrokerId,
                                AccountId = wallet.AccountId,
                                UsdVolume = actualBalance.UsdVolume,
                                Volume = actualBalance.Volume,
                                MinBalanceInUsd = 0m,
                                WalletTypes = wallet.WalletTypes
                            };
                        }

                        _internalWalletObserverMetrics.SetMetrics(newBalance);
                        newBalances.Add(newBalance);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, $"Cannot get balance of {wallet.WalletId} wallet");
                }
            }

            if (newBalances.Any())
            {
                await SetWalletsTypesAsync(newBalances);
                await _internalWalletStorage.SaveBalancesAsync(newBalances);
            }
        }

        public void Start()
        {
            _timer.Start();
        }

        private async Task SetWalletsTypesAsync(ICollection<InternalWalletBalance> walletBalances)
        {
            try
            {
                foreach (var wallet in walletBalances)
                {
                    if (Program.Settings.BonusServiceWalletId == wallet.WalletId)
                    {
                        wallet.WalletTypes |= InternalWalletTypes.Bonus;
                    }
                }
                
                await SetConverterWalletTypes(walletBalances);
                await SetHighYieldWalletTypes(walletBalances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set wallets types. {@ExMess}", ex.Message);
            }
        }

        private async Task SetConverterWalletTypes(ICollection<InternalWalletBalance> wallets)
        {
            try
            {
                var resp = await _rpcUnavailableRetryPolicy.ExecuteAsync(async () =>
                    _converterSettingsManager.GetLiquidityConverterSettingsAsync());
                var converterWalletIds = resp?.Settings?
                    .Select(s => s.BrokerWalletId)
                    .ToHashSet() ?? new HashSet<string>();

                foreach (var wallet in wallets)
                {
                    if (converterWalletIds.Contains(wallet.WalletId))
                    {
                        wallet.WalletTypes |= InternalWalletTypes.Converter;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set Converter wallet types. {@ExMessage}", ex.Message);
            }
        }
        
        private async Task SetHighYieldWalletTypes(ICollection<InternalWalletBalance> wallets)
        {
            try
            {
                var settings = await _rpcUnavailableRetryPolicy.ExecuteAsync(async () =>
                    await _highYieldService.GetEarnSettings());
                
                foreach (var wallet in wallets)
                {
                    if (wallet.WalletId == settings?.EarnBrokerWallet?.WalletId)
                    {
                        wallet.WalletTypes |= InternalWalletTypes.HighYield;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set HighYield wallet types. {@ExMessage}", ex.Message);
            }
        }
    }
}