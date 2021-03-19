using System;
using System.Collections.Generic;
using System.Text;
using SimpleTrading.SettingsReader;

namespace Service.BitGo.SignTransaction.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        public string SeqServiceUrl { get; set; }

        public string BitgoApiKey { get; set; }

        public string BitgoApiUrl { get; set; }

        public Dictionary<string, string> BitgoWalletPassphrase { get; set; }

        public string GetPassphraseByWalletId(string walletId)
        {
            if (BitgoWalletPassphrase == null)
            {
                return string.Empty;
            }

            if (BitgoWalletPassphrase.TryGetValue(walletId, out var pass))
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(pass)).Trim();
            }

            if (BitgoWalletPassphrase.TryGetValue("Default", out pass))
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(pass)).Trim();
            }

            return string.Empty;
        }
    }
}