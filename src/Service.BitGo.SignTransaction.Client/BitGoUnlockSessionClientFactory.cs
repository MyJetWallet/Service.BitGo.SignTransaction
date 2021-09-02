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
    public class BitGoUnlockSessionClientFactory
    {
        private readonly CallInvoker _channel;

        public BitGoUnlockSessionClientFactory(string grpcServiceUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(grpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public ISessionUnlockService GetSessionUnlockService() => _channel.CreateGrpcService<ISessionUnlockService>();
    }
}