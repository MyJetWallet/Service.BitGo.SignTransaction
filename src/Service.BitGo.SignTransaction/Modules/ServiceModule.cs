using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.BitGo;

namespace Service.BitGo.SignTransaction.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var bitgoClient = new BitGoClient(Program.Settings.BitgoApiKey, Program.Settings.BitgoApiUrl);
            bitgoClient.ThrowThenErrorResponse = false;

            builder
                .RegisterInstance(bitgoClient)
                .As<IBitGoClient>()
                .SingleInstance();
        }
    }
}