using PigApi.DTO;
using PigService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PigApi.Controller
{
    public class DayController : ApiController
    {
        public PigMoreStatisticsDao PigMoreStatisticsDao { get; set; }

        /// <summary>
        /// 今日交易， 购买数量，出售数量， 购买总额，出售总额，每条记录（）。
        /// </summary>
        /// <returns></returns>
        public async Task<object> ListTodayTrade(string userName)
        {
            var pigMoreList = await PigMoreStatisticsDao.ListTodayTrade(userName);

            var buyCount = 0;
            var sellCount = 0;
            var buyAmount = (decimal)0.0;
            var sellAmount = (decimal)0.0;
            var sellEarnings = (decimal)0.0;
            var list = new List<TodayTradeDTO>();
            pigMoreList.ForEach(it =>
            {
                if (it.BDate >= Utils.GetSmallestOfTheDate(DateTime.Now))
                {
                    buyCount++;
                    buyAmount += it.BQuantity * it.BTradeP;
                }
                if (it.SOrderId > 0)
                {
                    sellCount++;
                    sellAmount += it.SQuantity * it.STradeP;
                    sellEarnings += it.SQuantity * it.STradeP - it.BQuantity * it.BTradeP;
                }
                list.Add(new TodayTradeDTO() {
                    Name = it.Name,
                    BDate = it.BDate,
                    BQuantity = it.BQuantity,
                    BTradeP = it.BTradeP,
                    SQuantity = it.SQuantity,
                    SDate = it.SDate,
                    STradeP = it.STradeP,
                });
            });
            return 1;
        }
    }
}
