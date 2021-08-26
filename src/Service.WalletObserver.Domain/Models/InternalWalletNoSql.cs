using MyNoSqlServer.Abstractions;

namespace Service.WalletObserver.Domain.Models
{
    public class InternalWalletNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-liquidity-internalwallets";
        private static string GeneratePartitionKey(string walletId) => $"walletId : {walletId}";
        private static string GenerateRowKey(string asset) => $"asset : {asset}";
        public InternalWalletBalance WalletBalance { get; set; }
        
        public static InternalWalletNoSql Create(InternalWalletBalance walletBalance)
        {
            return new()
            {
                PartitionKey = GeneratePartitionKey(walletBalance.WalletId),
                RowKey = GenerateRowKey(walletBalance.Asset),
                WalletBalance = walletBalance
            };
        }
    }
}