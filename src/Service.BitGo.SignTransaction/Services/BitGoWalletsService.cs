using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;
using Service.BitGo.SignTransaction.Grpc;
using Service.BitGo.SignTransaction.Grpc.Models;
using Service.BitGo.SignTransaction.Utils;

// ReSharper disable InconsistentLogPropertyNaming

namespace Service.BitGo.SignTransaction.Services
{
    public class BitGoWalletsService : IBitGoWalletsService
    {
        private readonly ILogger<BitGoWalletsService> _logger;
        private readonly IMyNoSqlServerDataWriter<BitGoWalletNoSqlEntity> _writer;
        private readonly SymmetricEncryptionService _encryptionService;

        public BitGoWalletsService(ILogger<BitGoWalletsService> logger,
            IMyNoSqlServerDataWriter<BitGoWalletNoSqlEntity> writer, SymmetricEncryptionService encryptionService)
        {
            _logger = logger;
            _writer = writer;
            _encryptionService = encryptionService;
        }

        public async Task<BitGoWalletsList> GetBitGoWalletsList()
        {
            return new BitGoWalletsList()
            {
                Wallets = (await _writer.GetAsync())
                    .Select(e =>
                    {
                        e.Wallet.ApiKey = "***";
                        return e.Wallet;
                    }).ToList()
            };
        }

        public async Task<BitGoWallet> GetBitGoWallet(GetBitGoWalletRequest request)
        {
            var wallet = (await _writer.GetAsync(BitGoWalletNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                BitGoWalletNoSqlEntity.GenerateRowKey(request.WalletId))).Wallet;
            if (wallet != null)
            {
                wallet.ApiKey = "***";
            }

            return wallet;
        }

        public async Task AddBitGoWallet(BitGoWallet wallet)
        {
            using var action = MyTelemetry.StartActivity("Add BitGo wallet");
            wallet.ApiKey = _encryptionService.Encrypt(wallet.ApiKey);
            try
            {
                _logger.LogInformation("Add BitGoWallet: {jsonText}",
                    JsonConvert.SerializeObject(wallet, new ApiKeyHiddenJsonConverter(typeof(BitGoWallet))));

                ValidateWallet(wallet);

                var entity = BitGoWalletNoSqlEntity.Create(wallet);

                var existingItem = await _writer.GetAsync(entity.PartitionKey, entity.RowKey);
                if (existingItem != null) throw new Exception("Cannot add BitGo wallet. Already exist");

                await _writer.InsertAsync(entity);

                _logger.LogInformation("Added BitGo wallet: {jsonText}",
                    JsonConvert.SerializeObject(wallet, new ApiKeyHiddenJsonConverter(typeof(BitGoWallet))));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add BitGo wallet: {requestJson}",
                    JsonConvert.SerializeObject(wallet, new ApiKeyHiddenJsonConverter(typeof(BitGoWallet))));
                ex.FailActivity();
                throw;
            }
        }

        public async Task UpdateBitGoWallet(BitGoWallet wallet)
        {
            using var action = MyTelemetry.StartActivity("Update BitGo wallet");
            wallet.ApiKey = _encryptionService.Encrypt(wallet.ApiKey);
            try
            {
                _logger.LogInformation("Update BitGoWallet: {jsonText}",
                    JsonConvert.SerializeObject(wallet, new ApiKeyHiddenJsonConverter(typeof(BitGoWallet))));

                wallet.UpdatedDate = DateTime.Now;
                ValidateWallet(wallet);

                var entity = BitGoWalletNoSqlEntity.Create(wallet);

                var existingItem = await _writer.GetAsync(entity.PartitionKey, entity.RowKey);
                if (existingItem == null) throw new Exception("Cannot update BitGo wallet. Do not exist");

                await _writer.InsertOrReplaceAsync(entity);

                _logger.LogInformation("Updated BitGo wallet: {jsonText}",
                    JsonConvert.SerializeObject(wallet, new ApiKeyHiddenJsonConverter(typeof(BitGoWallet))));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot update BitGo wallet: {requestJson}",
                    JsonConvert.SerializeObject(wallet, new ApiKeyHiddenJsonConverter(typeof(BitGoWallet))));
                ex.FailActivity();
                throw;
            }
        }

        public async Task RemoveBitGoWallet(RemoveBitGoWalletRequest request)
        {
            using var action = MyTelemetry.StartActivity("Remove BitGo wallet");
            request.AddToActivityAsJsonTag("request");
            try
            {
                _logger.LogInformation("Remove BitGo wallet: {jsonText}",
                    JsonConvert.SerializeObject(request));

                var entity = await _writer.DeleteAsync(BitGoWalletNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                    BitGoWalletNoSqlEntity.GenerateRowKey(request.WalletId));

                if (entity != null)
                    _logger.LogInformation("Removed BitGo wallet: {jsonText}",
                        JsonConvert.SerializeObject(entity, new ApiKeyHiddenJsonConverter(typeof(BitGoWallet))));
                else
                    _logger.LogInformation("Unable to remove BitGo wallet, do not exist: {jsonText}",
                        JsonConvert.SerializeObject(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot remove BitGo wallet: {requestJson}",
                    JsonConvert.SerializeObject(request));
                ex.FailActivity();
                throw;
            }
        }

        private static void ValidateWallet(BitGoWallet wallet)
        {
            if (string.IsNullOrEmpty(wallet.BrokerId)) throw new Exception("Cannot add wallet with empty broker");
            if (string.IsNullOrEmpty(wallet.Id)) throw new Exception("Cannot add wallet with empty id");
            if (string.IsNullOrEmpty(wallet.CoinId)) throw new Exception("Cannot add wallet with coin id");
        }
    }
}