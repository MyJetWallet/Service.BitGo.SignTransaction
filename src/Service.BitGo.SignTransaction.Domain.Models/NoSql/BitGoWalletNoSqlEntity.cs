using MyNoSqlServer.Abstractions;

namespace Service.BitGo.SignTransaction.Domain.Models.NoSql
{
    public class BitGoWalletNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-bitgo-wallets";

        public static string GeneratePartitionKey(string brokerId) => brokerId;
        public static string GenerateRowKey(string walletId) => walletId;

        public BitGoWallet Wallet { get; set; }

        public static BitGoWalletNoSqlEntity Create(BitGoWallet wallet)
        {
            return new BitGoWalletNoSqlEntity
            {
                PartitionKey = GeneratePartitionKey(wallet.BrokerId),
                RowKey = GenerateRowKey(wallet.Id),
                Wallet = wallet
            };
        }
    }
}