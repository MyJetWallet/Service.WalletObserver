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
    }
}
