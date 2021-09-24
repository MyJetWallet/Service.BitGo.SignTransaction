using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using MyJetWallet.BitGo.Settings.Services;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;
using Service.BitGo.SignTransaction.Grpc;
using Service.BitGo.SignTransaction.Grpc.Models;

// ReSharper disable InconsistentLogPropertyNaming

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Service.BitGo.SignTransaction.Services
{
    public class SpendingLimitsService : ISpendingLimitsService
    {
        private readonly ILogger<SpendingLimitsService> _logger;
        private readonly IAssetMapper _assetMapper;
        private readonly SymmetricEncryptionService _encryptionService;
        private readonly IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> _myNoSqlServerUserDataReader;

        private readonly IBitGoClientService _bitGoClientService;

        public SpendingLimitsService(ILogger<SpendingLimitsService> logger,
            IAssetMapper assetMapper,
            IMyNoSqlServerDataReader<BitGoUserNoSqlEntity> myNoSqlServerUserDataReader,
            SymmetricEncryptionService encryptionService, IBitGoClientService bitGoClientService)
        {
            _logger = logger;
            _assetMapper = assetMapper;
            _myNoSqlServerUserDataReader = myNoSqlServerUserDataReader;
            _encryptionService = encryptionService;
            _bitGoClientService = bitGoClientService;
        }

        public async Task<GetBitGoWalletLimitsResponse> GetSpendingLimitsAsync(GetBitGoWalletLimitsRequest request)
        {
            try
            {
                var (coin, wallet) = _assetMapper.AssetToBitgoCoinAndWallet(request.BrokerId, request.AssetId);

                var client = _bitGoClientService.GetByUser(request.BrokerId, BitGoUserNoSqlEntity.TechSignerId, coin);
                if (client == null)
                {
                    return new GetBitGoWalletLimitsResponse
                    {
                        Success = false,
                        Error = $"Tech account is not configured, id = {BitGoUserNoSqlEntity.TechSignerId}, coin = {coin}"
                    };
                }
                
                var result = await client.GetSpendingLimitsForWalletAsync(coin, wallet);
                if (!result.Success)
                {
                    _logger.LogInformation(
                        $"Unable to get spending limits: {JsonConvert.SerializeObject(result.Error)}");
                    return new GetBitGoWalletLimitsResponse
                    {
                        Success = false,
                        Error = result.Error.Message
                    };
                }

                var limit = new SpendingLimit
                {
                    AssetId = request.AssetId,
                    BitgoCoin = coin
                };

                foreach (var spendingLimit in result.Data.Limits)
                {
                    if (spendingLimit.Coin == coin)
                    {
                        switch (spendingLimit.TimeWindow)
                        {
                            case "0":
                                limit.TransactionLimit =
                                    _assetMapper.ConvertAmountFromBitgo(coin, decimal.Parse(spendingLimit.LimitAmountString));
                                break;
                            case "3600":
                                limit.HourlyLimit =
                                    _assetMapper.ConvertAmountFromBitgo(coin, decimal.Parse(spendingLimit.LimitAmountString));
                                limit.HourlySpent =
                                    _assetMapper.ConvertAmountFromBitgo(coin, decimal.Parse(spendingLimit.AmountSpentString));
                                break;
                            case "86400":
                                limit.DailyLimit =
                                    _assetMapper.ConvertAmountFromBitgo(coin, decimal.Parse(spendingLimit.LimitAmountString));
                                limit.DailySpent =
                                    _assetMapper.ConvertAmountFromBitgo(coin, decimal.Parse(spendingLimit.AmountSpentString));
                                break;
                        }
                    }
                }

                return new GetBitGoWalletLimitsResponse
                {
                    Success = true,
                    Limit = limit
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSpendingLimitsAsync request error: {message}", ex.Message);
                throw;
            }
        }
    }
}