using log4net;
using SharpDapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigService
{
    public class PigMoreStatisticsDao : BaseDao
    {
        static ILog logger = LogManager.GetLogger(typeof(PigMoreStatisticsDao));

        public PigMoreStatisticsDao() : base()
        {
        }

        public async Task<List<PigMore>> ListTodayTrade(string userName)
        {
            var smallDate = Utils.GetSmallestOfTheDate(DateTime.Now);
            var bigDate = Utils.GetBiggestOfTheDate(DateTime.Now);
            var sql = $"select *, case when SState='{StateConst.Filled}' then SDate else BDate end OrderDate from t_pig_more where (BDate>=@SmallDate or SDate>=@SmallDate)";
            if (!string.IsNullOrEmpty(userName))
            {
                sql += $" and UserName='{userName}'";
            }
            sql += " order by OrderDate desc";
            return (await Database.QueryAsync<PigMore>(sql, new { SmallDate = smallDate })).ToList();
        }

        public async Task<List<PigMore>> ListBuy(string userName, string name, DateTime begin, DateTime end)
        {
            var smallDate = Utils.GetSmallestOfTheDate(DateTime.Now);
            var bigDate = Utils.GetBiggestOfTheDate(DateTime.Now);
            var sql = $"select * from t_pig_more where UserName=@UserName and Name=@Name and BDate>=@BeginDate or BDate<=@EndDate";
            return (await Database.QueryAsync<PigMore>(sql, new { UserName = userName, Name = name, BeginDate = begin, EndDate = end })).ToList();
        }

        public async Task<List<PigMore>> ListSell(string userName, string name, DateTime begin, DateTime end)
        {
            var smallDate = Utils.GetSmallestOfTheDate(DateTime.Now);
            var bigDate = Utils.GetBiggestOfTheDate(DateTime.Now);
            var sql = $"select * from t_pig_more where UserName=@UserName and Name=@Name and SDate>=@BeginDate or SDate<=@EndDate";
            return (await Database.QueryAsync<PigMore>(sql, new { UserName = userName, Name = name, BeginDate = begin, EndDate = end })).ToList();
        }
    }
}
