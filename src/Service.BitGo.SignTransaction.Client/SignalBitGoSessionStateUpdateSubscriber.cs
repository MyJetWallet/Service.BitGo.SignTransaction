using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.BitGo.SignTransaction.Domain.Models;

namespace Service.BitGo.SignTransaction.Client
{
    public class SignalBitGoSessionStateUpdateSubscriber : ISubscriber<SignalBitGoSessionStateUpdate>
    {
        private readonly List<Func<SignalBitGoSessionStateUpdate, ValueTask>> _list = new ();

        public SignalBitGoSessionStateUpdateSubscriber(
            MyServiceBusTcpClient client,
            string queueName,
            TopicQueueType queryType)
        {
            client.Subscribe(SignalBitGoSessionStateUpdate.ServiceBusTopicName, queueName, queryType, Handler);
        }

        private async ValueTask Handler(IMyServiceBusMessage data)
        {
            var item = Deserializer(data.Data);

            if (!_list.Any())
            {
                throw new Exception("Cannot handle event. No subscribers");
            }

            foreach (var callback in _list)
            {
                await callback.Invoke(item);
            }
        }


        public void Subscribe(Func<SignalBitGoSessionStateUpdate, ValueTask> callback)
        {
            _list.Add(callback);
        }

        private SignalBitGoSessionStateUpdate Deserializer(ReadOnlyMemory<byte> data) => data.ByteArrayToServiceBusContract<SignalBitGoSessionStateUpdate>();
    }
}