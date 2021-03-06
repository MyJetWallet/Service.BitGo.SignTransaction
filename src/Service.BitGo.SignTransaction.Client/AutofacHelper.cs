using Autofac;
using DotNetCoreDecorators;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;
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

        public static void RegisterBitGoUsersClient(this ContainerBuilder builder,
            string registerBitGoUsersGrpcServiceUrl)
        {
            var factory = new BitGoUsersClientFactory(registerBitGoUsersGrpcServiceUrl);

            builder.RegisterInstance(factory.GetBitGoUsersService()).As<IBitGoUsersService>().SingleInstance();
        }

        public static void RegisterBitGoWalletsClient(this ContainerBuilder builder,
            string registerBitGoWalletsGrpcServiceUrl)
        {
            var factory = new BitGoWalletsClientFactory(registerBitGoWalletsGrpcServiceUrl);

            builder.RegisterInstance(factory.GetBitGoWalletsService()).As<IBitGoWalletsService>().SingleInstance();
        }

        public static void RegisterPendingApprovalsClient(this ContainerBuilder builder,
            string registerBitGoPendingApprovalsGrpcServiceUrl)
        {
            var factory = new BitGoPendingApprovalClientFactory(registerBitGoPendingApprovalsGrpcServiceUrl);

            builder.RegisterInstance(factory.GetPendingApprovalService()).As<IPendingApprovalsService>()
                .SingleInstance();
        }

        public static void RegisterSpendingLimitsClient(this ContainerBuilder builder,
            string registerBitGoSpendingLimitsGrpcServiceUrl)
        {
            var factory = new BitGoSpendingLimitsClientFactory(registerBitGoSpendingLimitsGrpcServiceUrl);

            builder.RegisterInstance(factory.GetSpendingLimitsService()).As<ISpendingLimitsService>()
                .SingleInstance();
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

        public static void RegisterApiKeyVolumesClient(
            this ContainerBuilder builder,
            IMyNoSqlSubscriber myNoSqlSubscriber)
        {
            MyNoSqlReadRepository<ApiKeyVolumeNoSqlEntity> readRepository =
                new MyNoSqlReadRepository<ApiKeyVolumeNoSqlEntity>(myNoSqlSubscriber,
                    ApiKeyVolumeNoSqlEntity.TableName);
            builder.RegisterInstance(readRepository).As<IMyNoSqlServerDataReader<ApiKeyVolumeNoSqlEntity>>()
                .SingleInstance();
        }
    }
}