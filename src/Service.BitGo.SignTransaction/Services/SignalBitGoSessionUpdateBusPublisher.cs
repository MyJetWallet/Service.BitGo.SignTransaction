using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.TcpClient;
using Service.BitGo.SignTransaction.Domain.Models;

namespace Service.BitGo.SignTransaction.Services
{
    public class SignalBitGoSessionUpdateBusPublisher : IPublisher<SignalBitGoSessionStateUpdate>
    {
        private readonly MyServiceBusTcpClient _client;

        public SignalBitGoSessionUpdateBusPublisher(MyServiceBusTcpClient client)
        {
            _client = client;
            _client.CreateTopicIfNotExists(SignalBitGoSessionStateUpdate.ServiceBusTopicName);
        }

        public async ValueTask PublishAsync(SignalBitGoSessionStateUpdate valueToPublish)
        {
            var bytesToSend = valueToPublish.ServiceBusContractToByteArray();
            await _client.PublishAsync(SignalBitGoSessionStateUpdate.ServiceBusTopicName, bytesToSend, true);
        }
    }
}