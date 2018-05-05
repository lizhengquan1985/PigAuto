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

        public async Task<List<PigMore>> ListTodayTrade()
        {
            var smallDate = Utils.GetSmallestOfTheDate(DateTime.Now);
            var bigDate = Utils.GetBiggestOfTheDate(DateTime.Now);
            var sql = $"select * from t_pig_more where BDate>=@SmallDate or SDate>=@SmallDate";
            return (await Database.QueryAsync<PigMore>(sql, new { SmallDate = smallDate })).ToList();
        }
    }
}
