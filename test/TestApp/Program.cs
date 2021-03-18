using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf.Grpc.Client;
using Service.BitGo.SignTransaction.Client;
using Service.BitGo.SignTransaction.Grpc.Models;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();


            var factory = new BitGoSignTransactionClientFactory("http://localhost:82");
            var client = factory.GetPublishTransactionService();

            var resp = await client.SignAndSendTransactionAsync(new SendTransactionRequest()
            {
                BitgoWalletId = "6013e7b3d11c3704c6b47cf6191e74a8",
                BitgoCoin = "tbtc",
                Address = "2N3Y2Ev1N9UuVa6GBYzTpJRxEgjujV7dBcR",
                SequenceId = Guid.NewGuid().ToString("N"),
                Amount = "3000"
            });

            Console.WriteLine(JsonConvert.SerializeObject(resp));

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
