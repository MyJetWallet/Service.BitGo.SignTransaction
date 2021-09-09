using MyNoSqlServer.Abstractions;

namespace Service.BitGo.SignTransaction.Domain.Models.NoSql
{
    public class ApiKeyVolumeNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-bitgo-apikey-volumes";
        public static string GeneratePartitionKey(string brokerId) => brokerId;
        public static string GenerateRowKey(string apiKeyHash, string asset) => $"{apiKeyHash}:{asset}";

        public ApiKeyVolume Volume { get; set; }

        public static ApiKeyVolumeNoSqlEntity Create(ApiKeyVolume volume)
        {
            return new ApiKeyVolumeNoSqlEntity
            {
                PartitionKey = GeneratePartitionKey(volume.BrokerId),
                RowKey = GenerateRowKey(volume.ApiKeyHash, volume.Asset),
                Volume = volume
            };
        }
    }
}