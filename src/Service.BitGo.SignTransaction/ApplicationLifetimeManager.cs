using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using MyServiceBus.TcpClient;

namespace Service.BitGo.SignTransaction
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyServiceBusTcpClient _busTcpClient;
        private readonly MyNoSqlClientLifeTime _myNoSqlClient;

        public ApplicationLifetimeManager(
            IHostApplicationLifetime appLifetime,
            ILogger<ApplicationLifetimeManager> logger, 
            MyServiceBusTcpClient busTcpClient,
            MyNoSqlClientLifeTime myNoSqlClient) : base(appLifetime)
        {
            _logger = logger;
            _busTcpClient = busTcpClient;
            _myNoSqlClient = myNoSqlClient;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called");
            _myNoSqlClient.Start();
            _logger.LogInformation("MyNoSqlTcpClient is started");
            _busTcpClient.Start();
            _logger.LogInformation("MyServiceBusTcpClient is started");
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called");
            _myNoSqlClient.Stop();
            _logger.LogInformation("MyNoSqlTcpClient is stopped");
            _busTcpClient.Stop();
            _logger.LogInformation("MyServiceBusTcpClient is stop");
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called");
        }
    }
}