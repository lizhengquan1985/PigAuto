using PigPlatform.Model;
using SharpDapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigService
{
    public class KlineDao : BaseDao
    {
        public void CheckTable(string coin)
        {
            try
            {
                var createTableSql = $"CREATE TABLE `t_coin_{coin}` ( `RecordId` bigint(20) NOT NULL AUTO_INCREMENT,  " +
                    $" `Id` bigint(20) NOT NULL, " +
                    $" `Open` decimal(14, 6) NOT NULL, " +
                    $" `Close` decimal(14, 6) NOT NULL, " +
                    $" `Low` decimal(14, 6) NOT NULL, " +
                    $" `High` decimal(14, 6) NOT NULL, " +
                    $" `Vol` decimal(14, 6) NOT NULL, " +
                    $" `Count` decimal(14, 6) NOT NULL, " +
                    $" `CreateTime` datetime NOT NULL, " +
                    $" PRIMARY KEY(`RecordId`))" +
                    $" ENGINE = InnoDB DEFAULT CHARSET = utf8mb4; ";
                Database.Execute(createTableSql);
            }
            catch (Exception ex)
            {

            }
        }

        public void Record(string coin, HistoryKline line)
        {
            try
            {
                var sql = $"insert into t_coin_{coin}(Id, Open, Close, Low, High, Vol, Count, CreateTime) values({line.Id},{line.Open},{line.Close},{line.Low},{line.High},{line.Vol},{line.Count}, now())";
                Database.Execute(sql);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
