using PigPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRun
{
    public class JudgeBuyUtils
    {
        public bool CheckCalcMaxhuoluo(string coin, string toCoin, string minPeriod = "1min")
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
    }
}
