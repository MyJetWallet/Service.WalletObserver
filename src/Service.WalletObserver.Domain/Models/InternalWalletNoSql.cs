using MyNoSqlServer.Abstractions;

namespace Service.WalletObserver.Domain.Models
{
    public class InternalWalletNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "jetwallet-liquidity-internalwallets";
        private static string GeneratePartitionKey(string name) => $"name : {name}";
        private static string GenerateRowKey() => $"wallet";
        public InternalWallet Wallet { get; set; }
        
        public static InternalWalletNoSql Create(InternalWallet wallet)
        {
            return new()
            {
                PartitionKey = GeneratePartitionKey(wallet.Name),
                RowKey = GenerateRowKey(),
                Wallet = wallet
            };
        }
    }
}