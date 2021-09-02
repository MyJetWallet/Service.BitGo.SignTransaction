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
    public class BitGoUsersClientFactory
    {
        private readonly CallInvoker _channel;

        public BitGoUsersClientFactory(string bitGoUsersUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(bitGoUsersUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public IBitGoUsersService GetBitGoUsersService() => _channel.CreateGrpcService<IBitGoUsersService>();
    }
}