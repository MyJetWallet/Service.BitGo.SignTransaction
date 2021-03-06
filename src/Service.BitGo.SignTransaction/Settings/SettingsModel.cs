using MyYamlParser;

namespace Service.BitGo.SignTransaction.Settings
{
    public class SettingsModel
    {
        [YamlProperty("BitGoSignTransaction.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("BitGoSignTransaction.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("BitGoSignTransaction.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("BitGoSignTransaction.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("BitGoSignTransaction.BitgoExpressUrlMainNet")]
        public string BitgoExpressUrlMainNet { get; set; }
        
        [YamlProperty("BitGoSignTransaction.BitgoExpressUrlTestNet")]
        public string BitgoExpressUrlTestNet { get; set; }
    }
}