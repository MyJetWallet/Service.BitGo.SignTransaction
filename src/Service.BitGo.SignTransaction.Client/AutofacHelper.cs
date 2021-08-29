using Autofac;
using DotNetCoreDecorators;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.BitGo.SignTransaction.Client
{
    public static class AutofacHelper
    {
        public static void RegisterBitGoSignTransactionClient(this ContainerBuilder builder,
            string registerBitGoSignTransactionGrpcServiceUrl)
        {
            var factory = new BitGoSignTransactionClientFactory(registerBitGoSignTransactionGrpcServiceUrl);

            builder.RegisterInstance(factory.GetPublishTransactionService()).As<IPublishTransactionService>()
                .SingleInstance();
        }

        public static void RegisterBitGoUnlockSessionClient(this ContainerBuilder builder,
            string registerBitGoSignTransactionGrpcServiceUrl)
        {
            var factory = new BitGoUnlockSessionClientFactory(registerBitGoSignTransactionGrpcServiceUrl);

            builder.RegisterInstance(factory.GetSessionUnlockService()).As<ISessionUnlockService>().SingleInstance();
        }

        public static void RegisterSignalBitGoSessionStateUpdateSubscriber(this ContainerBuilder builder,
            MyServiceBusTcpClient client,
            string queueName,
            TopicQueueType queryType)
        {
            var subs = new SignalBitGoSessionStateUpdateSubscriber(client, queueName, queryType);

            builder
                .RegisterInstance(subs)
                .As<ISubscriber<SignalBitGoSessionStateUpdate>>()
                .SingleInstance();
        }
    }
}