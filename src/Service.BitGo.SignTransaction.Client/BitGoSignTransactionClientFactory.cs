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
    public class BitGoSignTransactionClientFactory
    {
        private readonly CallInvoker _channel;

        public BitGoSignTransactionClientFactory(string assetsDictionaryGrpcServiceUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(assetsDictionaryGrpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public IPublishTransactionService GetPublishTransactionService() =>
            _channel.CreateGrpcService<IPublishTransactionService>();
    }
}