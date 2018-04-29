using MySql.Data.MySqlClient;
using PigAccount;
using SharpDapper;
using SharpDapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigService
{
    public class PigMoreDao : BaseDao
    {
        public PigMoreDao()
        {
            string connectionString = AccountConfig.sqlConfig;
            var connection = new MySqlConnection(connectionString);
            Database = new DapperConnection(connection);

        }
        protected IDapperConnection Database { get; private set; }

        public void CreatePigMore(PigMore pigMore)
        {
            using (var tx = Database.BeginTransaction())
            {
                Database.Insert(pigMore);
                tx.Commit();
            }
        }

        public void UpdatePigMoreBuySuccess(long buyOrderId, decimal buyTradePrice, string buyOrderQuery)
        {
            using (var tx = Database.BeginTransaction())
            {
                var sql = $"update t_spot_record set BuyOrderQuery='{buyOrderQuery}', BuySuccess=1 , BuyTradePrice={buyTradePrice} where BuyOrderId ='{buyOrderId}'";
                Database.Execute(sql);
                tx.Commit();
            }
        }

        public List<PigMore> ListNotSetBuySuccess(string accountId, string coin)
        {
            var sql = $"select * from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and BuySuccess=0 and UserName='{AccountConfig.userName}'";
            return Database.Query<PigMore>(sql).ToList();
        }

        public List<PigMore> ListHasSellNotSetSellSuccess(string accountId, string coin)
        {
            var sql = $"select * from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and SellSuccess=0 and HasSell=1 and UserName='{AccountConfig.userName}'";
            return Database.Query<PigMore>(sql).ToList();
        }

        /// <summary>
        /// 获取没有出售的数量
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="coin"></param>
        /// <returns></returns>
        public int GetNoSellRecordCount(string accountId, string coin)
        {
            var sql = $"select count(1) from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and HasSell=0 and UserName='{AccountConfig.userName}' and celuo=0";
            return Database.Query<int>(sql).FirstOrDefault();
        }

        public List<PigMore> ListNoSellRecord(string accountId, string coin)
        {
            var sql = $"select * from t_spot_record where AccountId='{accountId}' and Coin = '{coin}' and HasSell=0 and UserName='{AccountConfig.userName}' and celuo=0";
            return Database.Query<PigMore>(sql).ToList();
        }

        public List<PigMore> ListAllNoSellRecord(string accountId)
        {
            var sql = $"select * from t_spot_record where AccountId='{accountId}' and HasSell=0 and UserName='{AccountConfig.userName}'";
            return Database.Query<PigMore>(sql).ToList();
        }

        public List<PigMore> ListPigMore(string accountId, string coin, List<string> stateList)
        {
            var states = "";
            stateList.ForEach(it =>
            {
                if (states != "")
                {
                    states += ",";
                }
                states += $"'{it}'";
            });
            var sql = $"select * from t_pig_more where AccountId='{accountId}' and Coin = '{coin}' and HasSell=0 and BState in({states}) and UserName='{AccountConfig.userName}'";
            return Database.Query<PigMore>(sql).ToList();
        }

        public int GetAllNoSellRecordCount()
        {
            var sql = $"select count(1) from t_spot_record where HasSell=0 and UserName='{AccountConfig.userName}'";
            return Database.Query<int>(sql).FirstOrDefault();
        }

        public void ChangeDataWhenSell(long id, decimal sellTotalQuantity, decimal sellOrderPrice, string sellOrderResult, string sellAnalyze, string sellOrderId)
        {
            if (sellAnalyze.Length > 4500)
            {
                sellAnalyze = sellAnalyze.Substring(0, 4500);
            }
            if (sellOrderResult.Length > 500)
            {
                sellOrderResult = sellOrderResult.Substring(0, 500);
            }

            using (var tx = Database.BeginTransaction())
            {
                var sql = $"update t_spot_record set HasSell=1, SellTotalQuantity={sellTotalQuantity}, sellOrderPrice={sellOrderPrice}, SellDate=now(), SellAnalyze='{sellAnalyze}', SellOrderResult='{sellOrderResult}',SellOrderId={sellOrderId} where Id = {id}";
                Database.Execute(sql);
                tx.Commit();
            }
        }

        public void UpdateTradeRecordSellSuccess(long sellOrderId, decimal sellTradePrice, string sellOrderQuery)
        {
            using (var tx = Database.BeginTransaction())
            {
                var sql = $"update t_spot_record set SellOrderQuery='{sellOrderQuery}', SellSuccess=1 , SellTradePrice={sellTradePrice} where SellOrderId ='{sellOrderId}'";
                Database.Execute(sql);
                tx.Commit();
            }
        }
    }
}
