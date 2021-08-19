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

        private List<InternalWalletBalance> _walletBalances = new List<InternalWalletBalance>();
        
        public InternalWalletStorage(IMyNoSqlServerDataWriter<InternalWalletNoSql> dataWriter,
            ILogger<InternalWalletStorage> logger)
        {
            _dataWriter = dataWriter;
            _logger = logger;
        }

        public async Task SaveWallet(InternalWalletBalance walletBalance)
        {
            await _dataWriter.InsertOrReplaceAsync(InternalWalletNoSql.Create(walletBalance));

            await ReloadSettings();

            _logger.LogInformation("Updated InternalWallet: {jsonText}",
                JsonConvert.SerializeObject(walletBalance));
        }
        
        public async Task<List<InternalWalletBalance>> GetWalletsAsync()
        {
            if (!_walletBalances.Any())
            {
                await ReloadSettings();
            }
            return _walletBalances;
        }
        
        public async Task<List<InternalWalletBalance>> GetWalletBalanceAsync(string walletName)
        {
            if (!_walletBalances.Any())
            {
                await ReloadSettings();
            }
            return _walletBalances.Where(e => e.WalletName == walletName).ToList();
        }
        
        public async Task<InternalWalletBalance> GetWalletBalanceAsync(string walletName, string asset)
        {
            if (!_walletBalances.Any())
            {
                await ReloadSettings();
            }
            return _walletBalances.FirstOrDefault(e => e.WalletName == walletName && e.Asset == asset);
        }

        public async Task RemoveWallet(string walletName)
        {
            var walletBalances = _walletBalances.Where(e => e.WalletName == walletName).ToList();
            
            if (walletBalances.Any())
            {
                foreach (var walletBalance in walletBalances)
                {
                    var noSqlEntity = InternalWalletNoSql.Create(new InternalWalletBalance() {WalletName = walletName, Asset = walletBalance.Asset});
                    await _dataWriter.DeleteAsync(noSqlEntity.PartitionKey, noSqlEntity.RowKey);
                }
                _logger.LogInformation("Removed wallet with name: {jsonText}", walletName);
                await ReloadSettings();
            }
        }
        
        private async Task ReloadSettings()
        {
            var wallets = (await _dataWriter.GetAsync()).ToList();
            var walletMap = new List<InternalWalletBalance>();
            walletMap.AddRange(wallets.Select(e=> e.WalletBalance));
            _walletBalances = walletMap;
        }

        public void Start()
        {
            ReloadSettings().GetAwaiter().GetResult();
        }
    }
}