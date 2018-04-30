using log4net;
using PigPlatform.Model;
using PigService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRun
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
                //Console.WriteLine($"总数：{data.Count}");

                nowPrice = data[0].Close;

                List<FlexPoint> flexPointList = new List<FlexPoint>();

                decimal openHigh = data[0].Open;
                decimal openLow =data[0].Open;
                long idHigh = data[0].Id;
                long idLow = data[0].Id;
                int lastHighOrLow = 0; // 1 high, -1: low
                foreach (var item in data)
                {
                    if (item.Open > openHigh)
                    {
                        openHigh = item.Open;
                        idHigh = item.Id;
                    }
                    if (item.Open < openLow)
                    {
                        openLow = item.Open;
                        idLow = item.Id;
                    }

                    if (openHigh >= openLow * (decimal)flexPercent)
                    {
                        var dtHigh = Utils.GetDateById(idHigh);
                        var dtLow = Utils.GetDateById(idLow);
                        // 相差了2%， 说明是一个节点了。
                        if (idHigh > idLow && lastHighOrLow != 1)
                        {
                            flexPointList.Add(new FlexPoint() { isHigh = true, open = openHigh, id = idHigh });
                            lastHighOrLow = 1;
                            openHigh = openLow;
                            idHigh = idLow;
                        }
                        else if (idHigh < idLow && lastHighOrLow != -1)
                        {
                            flexPointList.Add(new FlexPoint() { isHigh = false, open = openLow, id = idLow });
                            lastHighOrLow = -1;
                            openLow = openHigh;
                            idLow = idHigh;
                        }
                        else if (lastHighOrLow == 1)
                        {

                        }
                    }
                }

                if (flexPointList.Count != 0 && flexPointList[0].isHigh)
                {
                    // 
                    foreach (var item in data)
                    {
                        if (item.Id < flexPointList[0].id && lastLowPrice > item.Open)
                        {
                            lastLowPrice = item.Open;
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
