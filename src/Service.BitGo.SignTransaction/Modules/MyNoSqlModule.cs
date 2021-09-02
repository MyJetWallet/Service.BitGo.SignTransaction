using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.BitGo.SignTransaction.Domain.Models.NoSql;

namespace Service.BitGo.SignTransaction.Modules
{
    public class MyNoSqlModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<BitGoUserNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                BitGoUserNoSqlEntity.TableName);

            builder.RegisterMyNoSqlWriter<BitGoWalletNoSqlEntity>(Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                BitGoWalletNoSqlEntity.TableName);
        }
    }
}