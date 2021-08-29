using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using MyJetWallet.Sdk.Service;
using MyServiceBus.TcpClient;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Services;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Service.BitGo.SignTransaction.Modules
{
    public class ServiceModule : Module
    {
        public static ILogger ServiceBusLogger { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            var bitgoClient = new BitGoClient(Program.Settings.BitgoApiKey, Program.Settings.BitgoApiUrl);
            bitgoClient.ThrowThenErrorResponse = false;

            ServiceBusLogger = Program.LogFactory.CreateLogger(nameof(MyServiceBusTcpClient));

            var serviceBusClient = new MyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                ApplicationEnvironment.HostName ??
                $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");
            serviceBusClient.Log.AddLogException(ex =>
                ServiceBusLogger.LogInformation(ex, "Exception in MyServiceBusTcpClient"));
            serviceBusClient.Log.AddLogInfo(info => ServiceBusLogger.LogDebug($"MyServiceBusTcpClient[info]: {info}"));
            serviceBusClient.SocketLogs.AddLogInfo((context, msg) =>
                ServiceBusLogger.LogInformation(
                    $"MyServiceBusTcpClient[Socket {context?.Id}|{context?.ContextName}|{context?.Inited}][Info] {msg}"));
            serviceBusClient.SocketLogs.AddLogException((context, exception) =>
                ServiceBusLogger.LogInformation(exception,
                    $"MyServiceBusTcpClient[Socket {context?.Id}|{context?.ContextName}|{context?.Inited}][Exception] {exception.Message}"));
            builder.RegisterInstance(serviceBusClient).AsSelf().SingleInstance();

            builder
                .RegisterInstance(new SignalBitGoSessionUpdateBusPublisher(serviceBusClient))
                .As<IPublisher<SignalBitGoSessionStateUpdate>>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterInstance(bitgoClient)
                .As<IBitGoClient>()
                .SingleInstance();
        }
    }
}