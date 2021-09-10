using MyNoSqlServer.Abstractions;

namespace Service.BitGo.SignTransaction.Domain.Models.NoSql
{
    public class BitGoWalletNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-bitgo-wallets";

        public const string DefaultCoin = "default";

        public static string GeneratePartitionKey(string brokerId) => brokerId;

        public static string GenerateRowKey(string walletId, string coinId) =>
            $"{walletId}:{(string.IsNullOrEmpty(coinId) ? coinId : DefaultCoin)}";

        public BitGoWallet Wallet { get; set; }

        public static BitGoWalletNoSqlEntity Create(BitGoWallet wallet)
        {
            return new BitGoWalletNoSqlEntity
            {
                PartitionKey = GeneratePartitionKey(wallet.BrokerId),
                RowKey = GenerateRowKey(wallet.Id, wallet.CoinId),
                Wallet = wallet
            };
        }
    }
}