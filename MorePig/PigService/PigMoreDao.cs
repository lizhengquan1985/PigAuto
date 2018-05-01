using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PigAccount;
using PigPlatform.Model;
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
        public PigMoreDao():base()
        {
        }

        public void CreatePigMore(PigMore pigMore)
        {
            using (var tx = Database.BeginTransaction())
            {
                Database.Insert(pigMore);
                tx.Commit();
            }
        }

        #region 先查找出需要查询购买或者出售结果的记录， 然后查询结果，最后修改数据库记录

        /// <summary>
        /// 列出需要改变购买状态的记录
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public List<PigMore> ListNeedChangeBuyStatePigMore()
        {
            var states = $"'{StateConst.PartialFilled}','{StateConst.Filled}'";
            var sql = $"select * from t_pig_more where BState not in({states})";
            return Database.Query<PigMore>(sql).ToList();
        }

        /// <summary>
        /// 列出需要改变出售状态的
        /// </summary>
        /// <returns></returns>
        public List<PigMore> ListNeedChangeSellStatePigMore()
        {
            var states = $"'{StateConst.PartialFilled}','{StateConst.Filled}'";
            var sql = $"select * from t_pig_more where SState not in({states})";
            return Database.Query<PigMore>(sql).ToList();
        }

        public void UpdatePigMoreBuySuccess(long buyOrderId, HBResponse<OrderDetail> orderDetail, HBResponse<List<OrderMatchResult>> orderMatchResult, decimal buyTradePrice)
        {
            using (var tx = Database.BeginTransaction())
            {
                var sql = $"update t_pig_more set BTradeP={buyTradePrice}, BState='{orderDetail.Data.state}' ,BOrderDetail='{JsonConvert.SerializeObject(orderDetail)}', BOrderMatchResults='{JsonConvert.SerializeObject(orderMatchResult)}' where BOrderId ='{buyOrderId}'";
                Database.Execute(sql);
                tx.Commit();
            }
        }

        public void UpdateTradeRecordSellSuccess(long sellOrderId, HBResponse<OrderDetail> orderDetail, HBResponse<List<OrderMatchResult>> orderMatchResult, decimal sellTradePrice)
        {
            using (var tx = Database.BeginTransaction())
            {
                var sql = $"update t_spot_record set STradeP={sellTradePrice}, SState='{orderDetail.Data.state}' ,SOrderDetail='{JsonConvert.SerializeObject(orderDetail)}', SOrderMatchResults='{JsonConvert.SerializeObject(orderMatchResult)}' where SOrderId ='{sellOrderId}'";
                Database.Execute(sql);
                tx.Commit();
            }
        }

        #endregion

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
            var sql = $"select * from t_pig_more where AccountId='{accountId}' and Name = '{coin}' and HasSell=0 and BState in({states}) and UserName='{AccountConfig.userName}'";
            return Database.Query<PigMore>(sql).ToList();
        }

        public void ChangeDataWhenSell(long id, decimal sellQuantity, decimal sellOrderPrice, string sellOrderResult, string sFlex, long sellOrderId)
        {
            using (var tx = Database.BeginTransaction())
            {
                var sql = $"update t_pig_more set SQuantity={sellQuantity}, SOrderP={sellOrderPrice}, SDate=now(), SFlex='{sFlex}', SOrderResult='{sellOrderResult}',SOrderId={sellOrderId} where Id = {id}";
                Database.Execute(sql);
                tx.Commit();
            }
        }
    }
}
