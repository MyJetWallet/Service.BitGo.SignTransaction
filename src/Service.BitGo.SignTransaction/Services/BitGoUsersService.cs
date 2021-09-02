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
    public class BitGoUsersService : IBitGoUsersService
    {
        private readonly ILogger<BitGoUsersService> _logger;
        private readonly IMyNoSqlServerDataWriter<BitGoUserNoSqlEntity> _writer;
        private readonly SymmetricEncryptionService _encryptionService;

        public BitGoUsersService(ILogger<BitGoUsersService> logger,
            IMyNoSqlServerDataWriter<BitGoUserNoSqlEntity> writer, SymmetricEncryptionService encryptionService)
        {
            _logger = logger;
            _writer = writer;
            _encryptionService = encryptionService;
        }

        public async Task<BitGoUsersList> GetBitGoUsersList(GetBitGoUsersRequest request)
        {
            return new BitGoUsersList()
            {
                Users = (await _writer.GetAsync(BitGoUserNoSqlEntity.GeneratePartitionKey(request.BrokerId)))
                    .Select(e =>
                    {
                        e.User.ApiKey = "***";
                        return e.User;
                    }).ToList()
            };
        }

        public async Task<BitGoUser> GetBitGoUser(GetBitGoUserRequest request)
        {
            var user = (await _writer.GetAsync(BitGoUserNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                BitGoUserNoSqlEntity.GenerateRowKey(request.UserId))).User;
            if (user != null)
            {
                user.ApiKey = "***";
            }

            return user;
        }

        public async Task AddBitGoUser(BitGoUser user)
        {
            using var action = MyTelemetry.StartActivity("Add BitGo user");
            user.ApiKey = _encryptionService.Encrypt(user.ApiKey);
            try
            {
                _logger.LogInformation("Add BitGoUser: {jsonText}",
                    JsonConvert.SerializeObject(user, new ApiKeyHiddenJsonConverter(typeof(BitGoUser))));

                ValidateUser(user);

                var entity = BitGoUserNoSqlEntity.Create(user);

                var existingItem = await _writer.GetAsync(entity.PartitionKey, entity.RowKey);
                if (existingItem != null) throw new Exception("Cannot add BitGo user. Already exist");

                await _writer.InsertAsync(entity);

                _logger.LogInformation("Added BitGo user: {jsonText}",
                    JsonConvert.SerializeObject(user, new ApiKeyHiddenJsonConverter(typeof(BitGoUser))));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add BitGo user: {requestJson}",
                    JsonConvert.SerializeObject(user, new ApiKeyHiddenJsonConverter(typeof(BitGoUser))));
                ex.FailActivity();
                throw;
            }
        }

        public async Task UpdateBitGoUser(BitGoUser user)
        {
            using var action = MyTelemetry.StartActivity("Update BitGo user");
            user.ApiKey = _encryptionService.Encrypt(user.ApiKey);
            try
            {
                _logger.LogInformation("Update BitGoUser: {jsonText}",
                    JsonConvert.SerializeObject(user, new ApiKeyHiddenJsonConverter(typeof(BitGoUser))));

                ValidateUser(user);

                var entity = BitGoUserNoSqlEntity.Create(user);

                var existingItem = await _writer.GetAsync(entity.PartitionKey, entity.RowKey);
                if (existingItem == null) throw new Exception("Cannot update BitGo user. Do not exist");

                await _writer.InsertOrReplaceAsync(entity);

                _logger.LogInformation("Updated BitGo user: {jsonText}",
                    JsonConvert.SerializeObject(user, new ApiKeyHiddenJsonConverter(typeof(BitGoUser))));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot update BitGo user: {requestJson}",
                    JsonConvert.SerializeObject(user, new ApiKeyHiddenJsonConverter(typeof(BitGoUser))));
                ex.FailActivity();
                throw;
            }
        }

        public async Task RemoveBitGoUser(RemoveBitGoUserRequest request)
        {
            using var action = MyTelemetry.StartActivity("Remove BitGo user");
            request.AddToActivityAsJsonTag("request");
            try
            {
                _logger.LogInformation("Remove BitGo user: {jsonText}",
                    JsonConvert.SerializeObject(request));

                var entity = await _writer.DeleteAsync(BitGoUserNoSqlEntity.GeneratePartitionKey(request.BrokerId),
                    BitGoUserNoSqlEntity.GenerateRowKey(request.UserId));

                if (entity != null)
                    _logger.LogInformation("Removed BitGo user: {jsonText}",
                        JsonConvert.SerializeObject(entity, new ApiKeyHiddenJsonConverter(typeof(BitGoUser))));
                else
                    _logger.LogInformation("Unable to remove BitGo user, do not exist: {jsonText}",
                        JsonConvert.SerializeObject(request));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot remove BitGo user: {requestJson}",
                    JsonConvert.SerializeObject(request));
                ex.FailActivity();
                throw;
            }
        }

        private static void ValidateUser(BitGoUser user)
        {
            if (string.IsNullOrEmpty(user.BrokerId)) throw new Exception("Cannot add user with empty broker");
            if (string.IsNullOrEmpty(user.Id)) throw new Exception("Cannot add user with empty id");
            if (string.IsNullOrEmpty(user.BitGoId)) throw new Exception("Cannot add user with empty BitGoId");
        }
    }
}