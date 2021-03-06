﻿using log4net;
using PigPlatform.Model;
using PigService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRunService
{
    public class CoinAnalyze
    {
        static ILog logger = LogManager.GetLogger(typeof(CoinAnalyze));

        public static List<FlexPoint> Analyze(List<HistoryKline> data, out decimal lastLowPrice, out decimal nowPrice, decimal flexPercent)
        {
            nowPrice = 0; //现在的价格
            lastLowPrice = 999999999;

            try
            {
                nowPrice = data[0].Close;

                List<FlexPoint> flexPointList = new List<FlexPoint>();

                decimal closeHigh = data[0].Close;
                decimal closeLow = data[0].Close;
                long idHigh = data[0].Id;
                long idLow = data[0].Id;
                int lastHighOrLow = 0; // 1 high, -1: low
                foreach (var item in data)
                {
                    if (item.Close > closeHigh)
                    {
                        closeHigh = item.Close;
                        idHigh = item.Id;
                    }
                    if (item.Close < closeLow)
                    {
                        closeLow = item.Close;
                        idLow = item.Id;
                    }

                    if (closeHigh >= closeLow * flexPercent)
                    {
                        var dtHigh = Utils.GetDateById(idHigh);
                        var dtLow = Utils.GetDateById(idLow);
                        // 相差了flexPercent， 说明是一个节点了。
                        if (idHigh > idLow && lastHighOrLow != 1)
                        {
                            flexPointList.Add(new FlexPoint() { isHigh = true, close = closeHigh, id = idHigh });
                            lastHighOrLow = 1;
                            closeHigh = closeLow;
                            idHigh = idLow;
                        }// 改进
                        else if (idHigh < idLow && lastHighOrLow != -1)
                        {
                            flexPointList.Add(new FlexPoint() { isHigh = false, close = closeLow, id = idLow });
                            lastHighOrLow = -1;
                            closeLow = closeHigh;
                            idLow = idHigh;
                        }
                    }
                }

                if (flexPointList.Count != 0 && flexPointList[0].isHigh)
                {
                    foreach (var item in data)
                    {
                        if (item.Id < flexPointList[0].id && lastLowPrice > item.Close)
                        {
                            lastLowPrice = item.Close;
                        }
                    }
                }
                return flexPointList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
            return new List<FlexPoint>();
        }
    }
}
