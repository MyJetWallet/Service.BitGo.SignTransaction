using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using JetBrains.Annotations;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.BitGo.SignTransaction.Grpc;

namespace Service.BitGo.SignTransaction.Client
{
    [UsedImplicitly]
    public class BitGoWalletsClientFactory
    {
        private readonly CallInvoker _channel;

        public BitGoWalletsClientFactory(string bitGoWalletsUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(bitGoWalletsUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public IBitGoWalletsService GetBitGoWalletsService() => _channel.CreateGrpcService<IBitGoWalletsService>();
    }
}