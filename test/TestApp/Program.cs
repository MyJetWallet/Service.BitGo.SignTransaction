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


            //var factory = new BitGoSignTransactionClientFactory("http://localhost:82");
            var factory = new BitGoSignTransactionClientFactory("http://bitgo-sign-transaction.services.svc.cluster.local:80");
            
            var client = factory.GetPublishTransactionService();

            var request = new SendTransactionRequest()
            {
                BitgoWalletId = "604f5afa9ca16d000682de35465fc6e8",
                BitgoCoin = "tbtc",
                Address = "2N2VajawMvfKjhDnaPw1LLNUDVZRyzemXCC",
                SequenceId = Guid.NewGuid().ToString("N"),
                Amount = "10000"
            };

            var cmd = "";
            while (cmd != "exit")
            {
                try
                {
                    var resp = await client.SignAndSendTransactionAsync(request);
                    Console.WriteLine(JsonConvert.SerializeObject(resp));

                }catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }

                cmd = Console.ReadLine();

            }
            

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
