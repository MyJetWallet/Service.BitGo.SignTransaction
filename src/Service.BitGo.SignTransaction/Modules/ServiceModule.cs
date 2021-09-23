using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo.Settings.Ioc;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using MyServiceBus.TcpClient;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;
using Service.BitGo.SignTransaction.Services;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Service.BitGo.SignTransaction.Modules
{
    public class ServiceModule : Module
    {
        public static ILogger ServiceBusLogger { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder
                .CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            
            builder.RegisterBitgoSettingsReader(myNoSqlClient);

            builder
                .RegisterInstance(
                    new MyNoSqlReadRepository<BitGoWalletNoSqlEntity>(myNoSqlClient, BitGoWalletNoSqlEntity.TableName))
                .As<IMyNoSqlServerDataReader<BitGoWalletNoSqlEntity>>()
                .SingleInstance();

            builder
                .RegisterInstance(
                    new MyNoSqlReadRepository<BitGoUserNoSqlEntity>(myNoSqlClient, BitGoUserNoSqlEntity.TableName))
                .As<IMyNoSqlServerDataReader<BitGoUserNoSqlEntity>>()
                .SingleInstance();

            ServiceBusLogger = Program.LogFactory.CreateLogger(nameof(MyServiceBusTcpClient));
            
            var serviceBusClient = builder
                .RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                ApplicationEnvironment.HostName ?? 
                $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}",
                Program.LogFactory);
            
            builder
                .RegisterMyServiceBusPublisher<SignalBitGoSessionStateUpdate>(
                    serviceBusClient, SignalBitGoSessionStateUpdate.ServiceBusTopicName, true);

            builder
                .RegisterInstance(new SymmetricEncryptionService(Program.EnvSettings.GetEncryptionKey()))
                .AsSelf()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<BitGoClientService>()
                .As<IBitGoClientService>()
                .SingleInstance();
            
        }
    }
}