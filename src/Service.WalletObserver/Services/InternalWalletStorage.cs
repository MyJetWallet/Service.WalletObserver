using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.WalletObserver.Domain.Models;

namespace Service.WalletObserver.Services
{
    public class InternalWalletStorage : IStartable
    {
        private readonly ILogger<InternalWalletStorage> _logger;
        private readonly IMyNoSqlServerDataWriter<InternalWalletNoSql> _dataWriter;
        private readonly object _locker = new object();

        private List<InternalWalletBalance> _walletBalances = new List<InternalWalletBalance>();
        private List<(string, string)> _firingList = new List<(string, string)>();
        
        public InternalWalletStorage(IMyNoSqlServerDataWriter<InternalWalletNoSql> dataWriter,
            ILogger<InternalWalletStorage> logger)
        {
            _dataWriter = dataWriter;
            _logger = logger;
        }

        public async Task SaveWallet(List<InternalWalletBalance> snapshot)
        {
            ClearCollection(snapshot);
            RemoveFiringList();
            await _dataWriter.BulkInsertOrReplaceAsync(snapshot.Select(InternalWalletNoSql.Create));

            _logger.LogInformation("Updated InternalWallets: {jsonText}",
                JsonConvert.SerializeObject(snapshot));
            
            await ReloadSettings();
        }

        private void ClearCollection(List<InternalWalletBalance> snapshot)
        {
            snapshot.RemoveAll(e => string.IsNullOrWhiteSpace(e.Asset) &&
                                    string.IsNullOrWhiteSpace(e.WalletId) &&
                                    string.IsNullOrWhiteSpace(e.WalletName) &&
                                    string.IsNullOrWhiteSpace(e.BrokerId));
        }

        private void RemoveFiringList()
        {
            List<(string, string)> firingListCopy;
            lock (_locker)
            {
                firingListCopy = _firingList.Select(e => (new string(e.Item1), new string(e.Item2))).ToList();
                _firingList.Clear();
            }
            firingListCopy.ForEach(async e =>
            {
                await _dataWriter.DeleteAsync(e.Item1, e.Item2);
            });
        }

        public async Task<List<InternalWalletBalance>> GetWalletsSnapshot()
        {
            if (!_walletBalances.Any())
            {
                await ReloadSettings();
            }

            lock (_locker)
            {
                var snapshot = _walletBalances.Select(e => e.GetCopy()).ToList();
                return snapshot;
            }
        }

        public void UpsertBalance(InternalWalletBalance balance)
        {
            lock (_locker)
            {
                var elem = _walletBalances.FirstOrDefault(e => e.WalletId == balance.WalletId && e.Asset == balance.Asset);
                if (elem == null)
                {
                    _walletBalances.Add(balance);
                }
                else
                {
                    elem.MinBalanceInUsd = balance.MinBalanceInUsd;
                }
            }
            SaveWallet(_walletBalances).GetAwaiter().GetResult();
        }

        public Task RemoveWallet(string walletName)
        {
            lock (_locker)
            {
                _firingList.AddRange(_walletBalances.Where(e => e.WalletName == walletName).Select(e => (e.WalletId, e.Asset)));
                _walletBalances.RemoveAll(e => e.WalletName == walletName);
                _logger.LogInformation("Removed wallet with name: {jsonText}", walletName);
            }
            
            return Task.CompletedTask;
        }
        
        private async Task ReloadSettings()
        {
            var wallets = (await _dataWriter.GetAsync()).ToList();
            var walletMap = new List<InternalWalletBalance>();
            walletMap.AddRange(wallets.Select(e=> e.WalletBalance));
            lock (_locker)
            {
                _walletBalances = walletMap;
            }
        }

        public void Start()
        {
            ReloadSettings().GetAwaiter().GetResult();
        }
    }
}