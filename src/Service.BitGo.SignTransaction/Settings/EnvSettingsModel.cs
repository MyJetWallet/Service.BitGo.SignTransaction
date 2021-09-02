using System;
using System.Text;

namespace Service.BitGo.SignTransaction.Settings
{
    public class EnvSettingsModel
    {
        public string EncryptionKey { get; set; }

        public string GetEncryptionKey()
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(EncryptionKey)).Trim();
        }
    }
}