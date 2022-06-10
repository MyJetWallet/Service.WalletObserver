using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Service.WalletObserver.Domain.Models;

namespace Service.WalletObserver.Services
{
    public class InternalWalletStorage
    {
        private readonly ILogger<InternalWalletStorage> _logger;
        private readonly IMyNoSqlServerDataWriter<InternalWalletNoSql> _dataWriter;
        private List<InternalWalletBalance> _walletBalances = new List<InternalWalletBalance>();

        public InternalWalletStorage(
            IMyNoSqlServerDataWriter<InternalWalletNoSql> dataWriter,
            ILogger<InternalWalletStorage> logger
        )
        {
            _dataWriter = dataWriter;
            _logger = logger;
        }

        public async Task SaveBalancesAsync(IEnumerable<InternalWalletBalance> balances)
        {
            var cleared = ClearEmpty(balances).ToArray();
            await _dataWriter.BulkInsertOrReplaceAsync(cleared.Select(InternalWalletNoSql.Create).ToList());

            _logger.LogInformation("Updated InternalWallets count: {@Count}", cleared.Length);

            await ReloadAsync();
        }

        public async Task<List<InternalWalletBalance>> GetWalletsAsync()
        {
            if (_walletBalances == null || !_walletBalances.Any())
            {
                await ReloadAsync();
            }

            var snapshot = _walletBalances.Select(e => e.GetCopy()).ToList();

            return snapshot;
        }

        public async Task UpsertBalanceAsync(InternalWalletBalance balance)
        {
            await _dataWriter.InsertOrReplaceAsync(InternalWalletNoSql.Create(balance));
            await ReloadAsync();
        }

        public async Task RemoveBalancesAsync(string walletName)
        {
            var balances = _walletBalances
                .Where(w => w.WalletName == walletName)
                .ToArray();

            if (balances.Any())
            {
                foreach (var balance in balances)
                {
                    var noSql = InternalWalletNoSql.Create(balance);
                    await _dataWriter.DeleteAsync(noSql.PartitionKey, noSql.RowKey);
                }
            }

            await ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            var noSqlWallets = await _dataWriter.GetAsync();
            _walletBalances = new List<InternalWalletBalance>(noSqlWallets.Select(e => e.WalletBalance));
        }

        private static IEnumerable<InternalWalletBalance> ClearEmpty(IEnumerable<InternalWalletBalance> snapshot)
        {
            return snapshot.Where(e => string.IsNullOrWhiteSpace(e.Asset) &&
                                       string.IsNullOrWhiteSpace(e.WalletId) &&
                                       string.IsNullOrWhiteSpace(e.WalletName) &&
                                       string.IsNullOrWhiteSpace(e.BrokerId));
        }
    }
}