using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.WalletObserver.Settings
{
    public class SettingsModel
    {
        [YamlProperty("WalletObserver.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("WalletObserver.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("WalletObserver.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("WalletObserver.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("WalletObserver.BalancesGrpcServiceUrl")]
        public string BalancesGrpcServiceUrl { get; set; }

        [YamlProperty("WalletObserver.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("WalletObserver.BalanceUpdateTimerInSeconds")]
        public int BalanceUpdateTimerInSeconds { get; set; }
        
        [YamlProperty("WalletObserver.LiquidityConverterGrpcServiceUrl")]
        public string LiquidityConverterGrpcServiceUrl { get; set; }
        
        [YamlProperty("WalletObserver.BonusServiceWalletId")]
        public string BonusServiceWalletId { get; set; }

        [YamlProperty("WalletObserver.HighYieldEngineGrpcServiceUrl")]
        public string HighYieldEngineGrpcServiceUrl { get; set; }
    }
}
