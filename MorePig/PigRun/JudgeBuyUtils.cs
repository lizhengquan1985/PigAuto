using log4net;
using PigPlatform;
using PigPlatform.Model;
using PigService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRun
{
    public class JudgeBuyUtils
    {
        static ILog logger = LogManager.GetLogger(typeof(JudgeBuyUtils));

        public static bool CheckCalcMaxhuoluo(string coin, string toCoin, string minPeriod = "1min")
        {
            ResponseKline res = AnaylyzeApi().kline(coin + toCoin, minPeriod, 1440);
            Console.WriteLine(Utils.GetDateById(res.data[0].id));
            Console.WriteLine(Utils.GetDateById(res.data[res.data.Count - 1].id));
            decimal max = 0;
            decimal min = 999999;
            decimal now = res.data[0].open;
            foreach (var item in res.data)
            {
                if (max < item.open)
                {
                    max = item.open;
                }
                if (min > item.open)
                {
                    min = item.open;
                }
            }
            logger.Error($"火币回落, {max}, {min} {res.data[0].open}");
            return max > res.data[0].open * (decimal)1.02; // 是否下降2%
        }

        public static bool IsQuickRise(string coin, List<HistoryKline> historyKlines)
        {
            // 暂时判断 1个小时内是否上涨超过12%， 如果超过，则控制下
            var max = (decimal)0;
            var min = (decimal)9999999;
            var nowOpen = historyKlines[0].Open;
            for (var i = 0; i < 60; i++)
            {
                var item = historyKlines[i];
                if (max < item.Open)
                {
                    max = item.Open;
                }
                if (min > item.Open)
                {
                    min = item.Open;
                }
            }
            bool isQuickRise = false;
            if (max > min * (decimal)1.12)
            {
                if (nowOpen > min * (decimal)1.03)
                {
                    logger.Error($"一个小时内有大量的上涨，防止追涨，所以不能交易。coin:{coin}, nowOpen:{nowOpen}, min:{min}, max:{max}");
                    isQuickRise = true;
                }
            }
            return isQuickRise;
        }
    }
}
