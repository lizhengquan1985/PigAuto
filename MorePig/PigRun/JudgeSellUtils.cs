﻿using log4net;
using PigPlatform.Model;
using PigService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRun
{
    public class JudgeSellUtils
    {
        static ILog logger = LogManager.GetLogger(typeof(JudgeSellUtils));

        public static bool CheckCanSell(decimal buyPrice, decimal nearHigherOpen, decimal nowOpen, decimal gaoyuPercentSell = (decimal)1.03, bool needHuitou = true)
        {
            //item.BuyPrice, higher, itemNowOpen
            // if (item.BuyPrice * (decimal)1.05 < higher && itemNowOpen * (decimal)1.005 < higher)
            if (nowOpen < buyPrice * gaoyuPercentSell)
            {
                // 如果不高于 3% 没有意义
                return false;
            }

            if (nowOpen * (decimal)1.005 < nearHigherOpen && needHuitou)
            {
                // 表示回头趋势， 暂时定为 0.5% 就有回头趋势
                return true;
            }

            if (nowOpen * (decimal)1.001 < nearHigherOpen && !needHuitou)
            {
                return true;
            }

            return false;
        }

        public static decimal AnalyzeNeedSell(decimal comparePrice, DateTime compareDate, string coin, string toCoin, out decimal nowOpen, List<HistoryKline> data)
        {
            // 当前open
            nowOpen = 0;

            decimal higher = new decimal(0);

            try
            {
                nowOpen = data[0].Open;

                List<FlexPoint> flexPointList = new List<FlexPoint>();

                decimal openHigh = data[0].Open;
                decimal openLow = data[0].Open;
                long idHigh = 0;
                long idLow = 0;
                int lastHighOrLow = 0; // 1 high, -1: low
                foreach (var item in data)
                {
                    if (Utils.GetDateById(item.Id) < compareDate)
                    {
                        continue;
                    }

                    if (item.Open > higher)
                    {
                        higher = item.Open;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                Console.WriteLine("1111111111111111111111 over  " + ex.Message);
            }
            return higher;
        }
    }
}