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

        public async Task<object> ListTodayTrade()
        {
            var pigMoreList = await PigMoreStatisticsDao.ListTodayTrade();

            var buyCount = 0;
            var sellCount = 0;
            var buyAmount = (decimal)0.0;
            var sellAmount = (decimal)0.0;
            pigMoreList.ForEach(it =>
            {
                if (it.BDate >= Utils.GetSmallestOfTheDate(DateTime.Now))
                {
                    buyCount++;
                }
                if (it.SOrderId > 0)
                {
                    sellCount++;
                }
            });
        }
    }
}
