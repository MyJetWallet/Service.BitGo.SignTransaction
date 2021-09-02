using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Service.BitGo.SignTransaction.Grpc.Models
{
    [DataContract]
    public class SendTransactionRequest
    {
        [DataMember(Order = 1)] public AgentInfo Agent { get; set; } = new AgentInfo();
        [DataMember(Order = 2)] public string BitgoWalletId { get; set; }
        [DataMember(Order = 3)] public string BitgoCoin { get; set; }
        [DataMember(Order = 4)] public string Address { get; set; }
        [DataMember(Order = 5)] public string Amount { get; set; }
        [DataMember(Order = 6)] public string SequenceId { get; set; }
        [DataMember(Order = 7)] public string BrokerId { get; set; }

        [DataContract]
        public class AgentInfo
        {
            public static string AppName { get; set; }
            public static string AppEnvInfo { get; set; }

            public AgentInfo()
            {
                if (string.IsNullOrEmpty(AppName))
                {
                    AppName = Environment.GetEnvironmentVariable("APP_VERSION") ??
                              Assembly.GetEntryAssembly()?.GetName().Name ?? "none";
                    AppEnvInfo = Environment.GetEnvironmentVariable("ENV_INFO");
                }

                ApplicationName = AppName;
                ApplicationEnvInfo = AppEnvInfo;
            }

            [DataMember(Order = 1)] public string ApplicationName { get; set; }
            [DataMember(Order = 2)] public string ApplicationEnvInfo { get; set; }
        }
    }
}