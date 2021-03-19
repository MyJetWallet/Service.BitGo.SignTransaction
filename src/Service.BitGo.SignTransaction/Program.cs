using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Xml;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.BitGo.SignTransaction.Settings;
using SimpleTrading.SettingsReader;

namespace Service.BitGo.SignTransaction
{
    public class Program
    {
        public static SettingsModel Settings { get; private set; }

        public static ILoggerFactory LogFactory { get; private set; }

        public static Func<T> ReloadedSettings<T>(Func<SettingsModel, T> getter)
        {
            return () =>
            {
                var settings = Settings;
                var value = getter.Invoke(settings);
                return value;
            };
        }

        public static void Main(string[] args)
        {
            Console.Title = "MyJetWallet Service.BitGo.SignTransaction";

            Settings = ReaderSettings();

            using var loggerFactory = LogConfigurator.Configure("MyJetWallet", Settings.SeqServiceUrl);

            var logger = loggerFactory.CreateLogger<Program>();

            LogFactory = loggerFactory;

            try
            {
                logger.LogInformation("Application is being started");

                CreateHostBuilder(loggerFactory, args).Build().Run();

                logger.LogInformation("Application has been stopped");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Application has been terminated unexpectedly");
            }
        }

        private static SettingsModel ReaderSettings()
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            var result = configuration.Get<SettingsModel>();

            var pass = result.GetPassphraseByWalletId("Default");

            if (pass == string.Empty)
            {
                Console.WriteLine("Please set Env Variable BitgoWalletPassphrase__Default with default pass phase for wallets");
                throw new Exception("Please set Env Variable BitgoWalletPassphrase with pass phase for wallets and add 'Default' pass");
            }

            if (string.IsNullOrEmpty(result.BitgoApiKey))
            {
                Console.WriteLine("Please set Env Variable BitgoApiKey");
                throw new Exception("Please set Env Variable BitgoApiKey");
            }

            if (string.IsNullOrEmpty(result.BitgoApiUrl))
            {
                Console.WriteLine("Please set Env Variable BitgoApiUrl");
                throw new Exception("Please set Env Variable BitgoApiUrl");
            }

            return result;
        }

        public static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory, string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var httpPort = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";
                    var grpcPort = Environment.GetEnvironmentVariable("GRPC_PORT") ?? "80";

                    Console.WriteLine($"HTTP PORT: {httpPort}");
                    Console.WriteLine($"GRPC PORT: {grpcPort}");

                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, int.Parse(httpPort), o => o.Protocols = HttpProtocols.Http1);
                        options.Listen(IPAddress.Any, int.Parse(grpcPort), o => o.Protocols = HttpProtocols.Http2);
                    });

                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(loggerFactory);
                    services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                });
    }
}
