using System;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            // Console.Write("Press enter to start");
            // Console.ReadLine();

            Console.WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes("1q2waaAA1q2wasd!")));
            Console.WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes("Addda!1231!!!2$$3")));

            //
            //
            //
            // //var factory = new BitGoSignTransactionClientFactory("http://localhost:82");
            // var factory = new BitGoSignTransactionClientFactory("http://bitgo-sign-transaction.services.svc.cluster.local:80");
            //
            // var client = factory.GetPublishTransactionService();
            //
            // var request = new SendTransactionRequest()
            // {
            //     BitgoWalletId = "604f5afa9ca16d000682de35465fc6e8",
            //     BitgoCoin = "tbtc",
            //     Address = "2N2VajawMvfKjhDnaPw1LLNUDVZRyzemXCC",
            //     SequenceId = Guid.NewGuid().ToString("N"),
            //     Amount = "10000"
            // };
            //
            // var cmd = "";
            // while (cmd != "exit")
            // {
            //     try
            //     {
            //         var resp = await client.SignAndSendTransactionAsync(request);
            //         Console.WriteLine(JsonConvert.SerializeObject(resp));
            //
            //     }catch(Exception ex)
            //     {
            //         Console.WriteLine(ex);
            //     }
            //
            //     cmd = Console.ReadLine();
            //
            // }
            //
            //
            // Console.WriteLine("End");
            // Console.ReadLine();
        }
    }
}