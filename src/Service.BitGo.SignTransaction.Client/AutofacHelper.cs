using Autofac;
using Service.BitGo.SignTransaction.Grpc;
// ReSharper disable UnusedMember.Global

namespace Service.BitGo.SignTransaction.Client
{
    public static class AutofacHelper
    {
        public static void RegisterBitGoSignTransactionClient(this ContainerBuilder builder, string registerBitGoSignTransactionGrpcServiceUrl)
        {
            var factory = new BitGoSignTransactionClientFactory(registerBitGoSignTransactionGrpcServiceUrl);

            builder.RegisterInstance(factory.GetPublishTransactionService()).As<IPublishTransactionService>().SingleInstance();
        }
    }
}