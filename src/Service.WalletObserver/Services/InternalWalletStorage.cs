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

        private Dictionary<string, InternalWallet> _wallets = new Dictionary<string, InternalWallet>();
        
        public InternalWalletStorage(IMyNoSqlServerDataWriter<InternalWalletNoSql> dataWriter,
            ILogger<InternalWalletStorage> logger)
        {
            _dataWriter = dataWriter;
            _logger = logger;
        }

        public async Task SaveWallet(InternalWallet wallet)
        {
            await _dataWriter.InsertOrReplaceAsync(InternalWalletNoSql.Create(wallet));

            await ReloadSettings();

            _logger.LogInformation("Updated InternalWallet: {jsonText}",
                JsonConvert.SerializeObject(wallet));
        }
        
        public List<InternalWallet> GetWallets()
        {
            return _wallets.Values.ToList();
        }

        public async Task RemoveWallet(string walletName)
        {
            if (_wallets.TryGetValue(walletName, out var result))
            {
                var noSqlEntity = InternalWalletNoSql.Create(new InternalWallet() {Name = walletName});
                await _dataWriter.DeleteAsync(noSqlEntity.PartitionKey, noSqlEntity.RowKey);
                
                _logger.LogInformation("Removed wallet with name: {jsonText}", walletName);
                
                await ReloadSettings();
            }
        }
        
        private async Task ReloadSettings()
        {
            var wallets = (await _dataWriter.GetAsync()).ToList();
            var walletMap = new Dictionary<string, InternalWallet>();
            
            foreach (var wallet in wallets)
            {
                walletMap[wallet.Wallet.Name] =
                    wallet.Wallet;
            }
            _wallets = walletMap;
        }

        public void Start()
        {
            ReloadSettings().GetAwaiter().GetResult();
        }
    }
}