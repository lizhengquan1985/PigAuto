﻿using log4net;
using PigPlatform;
using PigPlatform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRunService
{
    public class JudgeBuyUtils
    {
        static ILog logger = LogManager.GetLogger(typeof(JudgeBuyUtils));

        public static bool CheckCanBuy(decimal nowOpen, decimal nearLowOpen)
        {
            //nowOpen > flexPointList[0].open * (decimal)1.005 && nowOpen < flexPointList[0].open * (decimal)1.01
            var canbuy = nowOpen > nearLowOpen * (decimal)1.005 && nowOpen < nearLowOpen * (decimal)1.01;
            logger.Error($"------- {canbuy}还不能购买 nowOpen:{nowOpen}, nearLowOpen:{nearLowOpen}");
            return canbuy;
        }

        public static bool CheckCalcMaxhuoluo(List<HistoryKline> data)
        {
            decimal max = 0;
            decimal min = 999999;
            decimal nowPrice = data[0].Close;
            foreach (var item in data)
            {
                if (max < item.Close)
                {
                    max = item.Close;
                }
                if (min > item.Close)
                {
                    min = item.Close;
                }
            }
            logger.Error($"火币回落(是否下降2%), max:{max}, min:{min}, nowPrice:{data[0].Close}, 比率：{nowPrice / max}");
            return max > nowPrice * (decimal)1.02; // 是否下降2%
        }

        public static bool IsQuickRise(string coin, List<HistoryKline> historyKlines)
        {
            // 暂时判断 1个小时内是否上涨超过12%， 如果超过，则控制下
            var max = (decimal)0;
            var min = (decimal)9999999;
            var nowClose = historyKlines[0].Close;
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
                if (nowClose > min * (decimal)1.04)
                {
                    logger.Error($"一个小时内有大量的上涨，防止追涨，所以不能交易。coin:{coin}, nowClose:{nowClose}, min:{min}, max:{max}");
                    isQuickRise = true;
                }
            }
            return isQuickRise;
        }
    }
}
