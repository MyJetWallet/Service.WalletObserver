using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Elasticsearch.Net.Specification.SnapshotApi;
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
        
        public InternalWalletStorage(IMyNoSqlServerDataWriter<InternalWalletNoSql> dataWriter,
            ILogger<InternalWalletStorage> logger)
        {
            _dataWriter = dataWriter;
            _logger = logger;
        }

        public async Task SaveWallet(List<InternalWalletBalance> snapshot)
        {
            await _dataWriter.CleanAndBulkInsertAsync(snapshot.Select(InternalWalletNoSql.Create));

            _logger.LogInformation("Updated InternalWallets: {jsonText}",
                JsonConvert.SerializeObject(snapshot));
            
            await ReloadSettings();
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
                var elem = _walletBalances.FirstOrDefault(e => e.WalletId == balance.WalletId);
                if (elem != null)
                {
                    elem = balance;
                }
                else
                {
                    _walletBalances.Add(balance);
                }
            }
        }

        public async Task RemoveWallet(string walletName)
        {
            lock (_locker)
            {
                _walletBalances.RemoveAll(e => e.WalletName == walletName);
                _logger.LogInformation("Removed wallet with name: {jsonText}", walletName);
            }
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