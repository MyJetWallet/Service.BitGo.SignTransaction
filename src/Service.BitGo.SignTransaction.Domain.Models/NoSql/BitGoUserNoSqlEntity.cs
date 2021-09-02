using MyNoSqlServer.Abstractions;

namespace Service.BitGo.SignTransaction.Domain.Models.NoSql
{
    public class BitGoUserNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-bitgo-users";

        public const string TechSignerId = "TechSigner";

        public const string TechPendingApprovalsViewerId = "TechPendingApprovalsViewer";

        public static string GeneratePartitionKey(string brokerId) => brokerId;
        public static string GenerateRowKey(string userId) => userId;

        public BitGoUser User { get; set; }

        public static BitGoUserNoSqlEntity Create(BitGoUser user)
        {
            return new BitGoUserNoSqlEntity
            {
                PartitionKey = GeneratePartitionKey(user.BrokerId),
                RowKey = GenerateRowKey(user.Id),
                User = user
            };
        }
    }
}