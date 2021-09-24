using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using MyJetWallet.BitGo.Settings.NoSql;
using MyNoSqlServer.Abstractions;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;

namespace Service.BitGo.SignTransaction.Services
{
    public interface IBitGoClientService
    {
        IBitGoApi GetByUser(string brokerId, string userId, string coinId);
    }

    public class BitGoClientService : IBitGoClientService
    {
        private readonly IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> _bitgoUserReader;
        private readonly ILogger<BitGoClientService> _logger;
        private readonly SymmetricEncryptionService _encryptionService;
        private readonly IMyNoSqlServerDataReader<BitgoCoinEntity> _bitgoCointReader;
        
        private readonly Dictionary<string, IBitGoApi> _clients = new Dictionary<string, IBitGoApi>();

        public BitGoClientService(IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> bitgoUserReader, 
            ILogger<BitGoClientService> logger, 
            SymmetricEncryptionService encryptionService, 
            IMyNoSqlServerDataReader<BitgoCoinEntity> bitgoCointReader)
        {
            _bitgoUserReader = bitgoUserReader;
            _logger = logger;
            _encryptionService = encryptionService;
            _bitgoCointReader = bitgoCointReader;
        }

        public IBitGoApi GetByUser(string brokerId, string userId, string coinId)
        {
            var bitGoUser = _bitgoUserReader.Get(
                                BitGoUserNoSqlEntity.GeneratePartitionKey(brokerId),
                                BitGoUserNoSqlEntity.GenerateRowKey(BitGoUserNoSqlEntity.TechSignerId,
                                    coinId)) ??
                            _bitgoUserReader.Get(
                                BitGoUserNoSqlEntity.GeneratePartitionKey(brokerId),
                                BitGoUserNoSqlEntity.GenerateRowKey(BitGoUserNoSqlEntity.TechSignerId,
                                    BitGoUserNoSqlEntity.DefaultCoin));

            var apiKeyEnc = bitGoUser?.User?.ApiKey;
            
            if (string.IsNullOrEmpty(apiKeyEnc))
            {
                _logger.LogError("Tech account is not configured, id = {techSignerName}",
                    BitGoUserNoSqlEntity.TechSignerId);
                return null;
            }

            lock (_clients)
            {
                if (_clients.TryGetValue(apiKeyEnc, out var api))
                    return api;
            }

            var coin = _bitgoCointReader.Get(BitgoCoinEntity.GeneratePartitionKey(),
                BitgoCoinEntity.GenerateRowKey(coinId));

            if (coin == null)
            {
                _logger.LogError("Cannot fond bitgo coin id = {symbol}", coinId);
                return null;
            }
            
            var apiKey = _encryptionService.Decrypt(bitGoUser.User.ApiKey);

            var client = new BitGoClient(
                Program.Settings.BitgoExpressUrlMainNet, Program.Settings.BitgoExpressUrlTestNet,
                apiKey, apiKey);

            lock (_clients)
            {
                _clients[apiKey] = coin.IsMainNet ? client.MainNet : client.TestNet;
                return _clients[apiKey];
            }
        }
    }
}