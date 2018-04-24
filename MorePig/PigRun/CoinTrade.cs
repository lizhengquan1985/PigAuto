using log4net;
using PigPlatform;
using PigPlatform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRun
{
    public class CoinTrade
    {
        static ILog logger = LogManager.GetLogger(typeof(CoinTrade));

        public static void Run(CommonSymbols symbol, PlatformApi api)
        {
            var accountId = AccountConfig.mainAccountId;
            var key = HistoryKlinePools.GetKey(symbol, "1min");
            var historyKlines = HistoryKlinePools.Get(key);

            // 获取最近行情
            decimal lastLowPrice;
            decimal nowPrice;
            // 分析是否下跌， 下跌超过一定数据，可以考虑
            decimal flexPercent = (decimal)1.04;
            var flexPointList = CoinAnalyze.Analyze(historyKlines, out lastLowPrice, out nowPrice, flexPercent);
            if (flexPointList == null || flexPointList.Count <= 1)
            {
                flexPercent = (decimal)1.03;
                flexPointList = CoinAnalyze.Analyze(historyKlines, out lastLowPrice, out nowPrice, flexPercent);
            }
            int celuo = 0; //默认 下面一截都是在平稳时候的策略
            var celue2FlextPointList = new List<FlexPoint>();
            if (flexPointList == null || flexPointList.Count <= 1)
            {
                flexPercent = (decimal)1.02;
                celue2FlextPointList = CoinAnalyze.Analyze(historyKlines, out lastLowPrice, out nowPrice, flexPercent);
                celuo = 1;
                if (flexPointList == null || flexPointList.Count < 1)
                {
                    flexPercent = (decimal)1.015;
                    celue2FlextPointList = CoinAnalyze.Analyze(historyKlines, out lastLowPrice, out nowPrice, flexPercent);
                }
            }
            if (flexPointList.Count == 0 && celue2FlextPointList.Count == 0)
            {
                logger.Error($"--------------> 分析结果数量为0 {coin}");
                return;
            }

            decimal recommendAmount = GetRecommendBuyAmount(coin);
            Console.Write($"spot--------> 开始 {coin}  推荐额度：{decimal.Round(recommendAmount, 2)} ");

            //try
            //{
            //    // 查询出结果还没好的数据， 去搜索一下
            //    var noSetBuySuccess = new CoinDao().ListNotSetBuySuccess(accountId, coin);
            //    foreach (var item in noSetBuySuccess)
            //    {
            //        QueryDetailAndUpdate(item.BuyOrderId);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.Message, ex);
            //}

            //try
            //{
            //    // 查询出结果还没好的数据， 去搜索一下
            //    var noSetSellSuccess = new CoinDao().ListHasSellNotSetSellSuccess(accountId, coin);
            //    foreach (var item in noSetSellSuccess)
            //    {
            //        Console.WriteLine("----------> " + JsonConvert.SerializeObject(item));
            //        QuerySellDetailAndUpdate(item.SellOrderId);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.Message, ex);
            //}

            if (CheckBalance() && recommendAmount > (decimal)0.3 && !IsQuickRise(coin, res))
            {
                if (celuo > 0 && celue2FlextPointList.Count > 0 && !celue2FlextPointList[0].isHigh && recommendAmount > (decimal)1.3)
                {
                    var celue1count = new CoinDao().GetNoSellRecordCountForCelue1(accountId, coin);
                    if (celue1count <= 0 && new CoinAnalyze().CheckCalcMaxhuoluo(coin, "usdt", "5min"))
                    {
                        // 说明可以购入策略1的方案
                        recommendAmount = recommendAmount / 4;
                        decimal buyQuantity = recommendAmount / nowOpen;
                        buyQuantity = decimal.Round(buyQuantity, GetBuyQuantityPrecisionNumber(coin));
                        decimal orderPrice = decimal.Round(nowOpen * (decimal)1.005, getPrecisionNumber(coin));
                        ResponseOrder order = new AccountOrder().NewOrderBuy(accountId, buyQuantity, orderPrice, null, coin, "usdt");
                        if (order.status != "error")
                        {
                            new CoinDao().CreateSpotRecord(new SpotRecord()
                            {
                                Coin = coin,
                                UserName = AccountConfig.userName,
                                BuyTotalQuantity = buyQuantity,
                                BuyOrderPrice = orderPrice,
                                BuyDate = DateTime.Now,
                                HasSell = false,
                                BuyOrderResult = JsonConvert.SerializeObject(order),
                                BuyAnalyze = JsonConvert.SerializeObject(flexPointList),
                                AccountId = accountId,
                                BuySuccess = false,
                                BuyTradePrice = 0,
                                BuyOrderId = order.data,
                                BuyOrderQuery = "",
                                SellAnalyze = "",
                                SellOrderId = "",
                                SellOrderQuery = "",
                                SellOrderResult = "",
                                Celuo = celuo
                            });
                            ClearData();
                            // 下单成功马上去查一次
                            QueryDetailAndUpdate(order.data);
                        }
                        else
                        {
                            logger.Error($"下单结果（策略1） coin{coin} accountId:{accountId}  购买数量{buyQuantity} nowOpen{nowOpen} {JsonConvert.SerializeObject(order)}");
                            logger.Error($"下单结果（策略1） 分析 {JsonConvert.SerializeObject(flexPointList)}");
                        }
                    }
                }
                if (flexPointList.Count > 0 && !flexPointList[0].isHigh)
                {
                    var noSellCount = new CoinDao().GetNoSellRecordCount(accountId, coin);
                    // 最后一次是高位, 没有交易记录， 则判断是否少于最近的6%
                    if (noSellCount <= 0 && CheckCanBuy(nowOpen, flexPointList[0].open, celuo) && new CoinAnalyze().CheckCalcMaxhuoluo(coin, "usdt", "5min"))
                    {
                        // 可以考虑
                        decimal buyQuantity = recommendAmount / nowOpen;
                        buyQuantity = decimal.Round(buyQuantity, GetBuyQuantityPrecisionNumber(coin));
                        decimal orderPrice = decimal.Round(nowOpen * (decimal)1.005, getPrecisionNumber(coin));
                        ResponseOrder order = new AccountOrder().NewOrderBuy(accountId, buyQuantity, orderPrice, null, coin, "usdt");
                        if (order.status != "error")
                        {
                            new CoinDao().CreateSpotRecord(new SpotRecord()
                            {
                                Coin = coin,
                                UserName = AccountConfig.userName,
                                BuyTotalQuantity = buyQuantity,
                                BuyOrderPrice = orderPrice,
                                BuyDate = DateTime.Now,
                                HasSell = false,
                                BuyOrderResult = JsonConvert.SerializeObject(order),
                                BuyAnalyze = JsonConvert.SerializeObject(flexPointList),
                                AccountId = accountId,
                                BuySuccess = false,
                                BuyTradePrice = 0,
                                BuyOrderId = order.data,
                                BuyOrderQuery = "",
                                SellAnalyze = "",
                                SellOrderId = "",
                                SellOrderQuery = "",
                                SellOrderResult = "",
                                Celuo = celuo
                            });
                            ClearData();
                            // 下单成功马上去查一次
                            QueryDetailAndUpdate(order.data);
                        }
                        else
                        {
                            logger.Error($"下单结果 coin{coin} accountId:{accountId}  购买数量{buyQuantity} nowOpen{nowOpen} {JsonConvert.SerializeObject(order)}");
                            logger.Error($"下单结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                        }
                    }

                    if (noSellCount > 0)
                    {
                        // 获取最小的那个， 如果有，
                        decimal minBuyPrice = 9999;
                        var noSellList = new CoinDao().ListNoSellRecord(accountId, coin);
                        foreach (var item in noSellList)
                        {
                            if (item.BuyOrderPrice < minBuyPrice)
                            {
                                minBuyPrice = item.BuyOrderPrice;
                            }
                        }

                        // 再少于5%， 
                        var per = new CoinAnalyze().CalcPercent(coin);
                        decimal pecent = getCalcPencent222(per, flexPointList.Count);//noSellCount >= 15 ? (decimal)1.03 : (decimal)1.025;
                        if (nowOpen * pecent < minBuyPrice)
                        {
                            decimal buyQuantity = recommendAmount / nowOpen;
                            buyQuantity = decimal.Round(buyQuantity, GetBuyQuantityPrecisionNumber(coin));
                            decimal orderPrice = decimal.Round(nowOpen * (decimal)1.005, getPrecisionNumber(coin));
                            ResponseOrder order = new AccountOrder().NewOrderBuy(accountId, buyQuantity, orderPrice, null, coin, "usdt");
                            if (order.status != "error")
                            {
                                new CoinDao().CreateSpotRecord(new SpotRecord()
                                {
                                    Coin = coin,
                                    UserName = AccountConfig.userName,
                                    BuyTotalQuantity = buyQuantity,
                                    BuyOrderPrice = orderPrice,
                                    BuyDate = DateTime.Now,
                                    HasSell = false,
                                    BuyOrderResult = JsonConvert.SerializeObject(order),
                                    BuyAnalyze = JsonConvert.SerializeObject(flexPointList),
                                    AccountId = accountId,
                                    BuySuccess = false,
                                    BuyTradePrice = 0,
                                    BuyOrderId = order.data,
                                    BuyOrderQuery = "",
                                    SellAnalyze = "",
                                    SellOrderId = "",
                                    SellOrderQuery = "",
                                    SellOrderResult = ""
                                });
                                ClearData();
                                // 下单成功马上去查一次
                                QueryDetailAndUpdate(order.data);
                            }
                            else
                            {
                                logger.Error($"下单结果 coin{coin} accountId:{accountId}  购买数量{buyQuantity} nowOpen{nowOpen} {JsonConvert.SerializeObject(order)}");
                                logger.Error($"下单结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                            }
                        }
                    }
                }
            }

            //ResponseKline res = new AnaylyzeApi().kline(coin + "usdt", "1min", 1440);
            // 查询数据库中已经下单数据，如果有，则比较之后的最高值，如果有，则出售

            {
                var needSellList = new CoinDao().ListBuySuccessAndNoSellRecord(accountId, coin, 0);
                SpotRecord last = null;
                foreach (var item in needSellList)
                {
                    if (last == null || item.BuyDate > last.BuyDate)
                    {
                        last = item;
                    }
                }

                foreach (var item in needSellList)
                {
                    // 分析是否 大于
                    decimal itemNowOpen = 0;
                    decimal higher = new CoinAnalyze().AnalyzeNeedSell(item.BuyOrderPrice, item.BuyDate, coin, "usdt", out itemNowOpen, res);

                    decimal gaoyuPercentSell = (decimal)1.035;
                    if (needSellList.Count > 10)
                    {
                        gaoyuPercentSell = (decimal)1.050;
                    }
                    else if (needSellList.Count > 9)
                    {
                        gaoyuPercentSell = (decimal)1.048;
                    }
                    else if (needSellList.Count > 8)
                    {
                        gaoyuPercentSell = (decimal)1.046;
                    }
                    else if (needSellList.Count > 7)
                    {
                        gaoyuPercentSell = (decimal)1.044;
                    }
                    else if (needSellList.Count > 6)
                    {
                        gaoyuPercentSell = (decimal)1.042;
                    }
                    else if (needSellList.Count > 5)
                    {
                        gaoyuPercentSell = (decimal)1.04;
                    }

                    bool needHuitou = true;// 如果很久没有出售过,则要考虑不需要回头
                    if (flexPercent < (decimal)1.04)
                    {
                        gaoyuPercentSell = (decimal)1.035;
                        if (flexPointList.Count <= 2 && last.BuyDate < DateTime.Now.AddDays(-1))
                        {
                            // 1天都没有交易. 并且波动比较小. 则不需要回头
                            needHuitou = false;
                        }
                    }

                    var canSell = CheckCanSell(item.BuyOrderPrice, higher, itemNowOpen, gaoyuPercentSell, needHuitou);

                    if (canSell)
                    {
                        decimal sellQuantity = item.BuyTotalQuantity * (decimal)0.99;
                        sellQuantity = decimal.Round(sellQuantity, getSellPrecisionNumber(coin));
                        if (coin == "xrp" && sellQuantity < 1)
                        {
                            sellQuantity = 1;
                        }
                        // 出售
                        decimal sellPrice = decimal.Round(itemNowOpen * (decimal)0.985, getPrecisionNumber(coin));
                        ResponseOrder order = new AccountOrder().NewOrderSell(accountId, sellQuantity, sellPrice, null, coin, "usdt");
                        if (order.status != "error")
                        {
                            new CoinDao().ChangeDataWhenSell(item.Id, sellQuantity, sellPrice, JsonConvert.SerializeObject(order), JsonConvert.SerializeObject(flexPointList), order.data);
                            // 下单成功马上去查一次
                            QuerySellDetailAndUpdate(order.data);
                        }
                        else
                        {
                            logger.Error($"出售结果 coin{coin} accountId:{accountId}  出售数量{sellQuantity} itemNowOpen{itemNowOpen} higher{higher} {JsonConvert.SerializeObject(order)}");
                            logger.Error($"出售结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                        }
                        ClearData();
                    }
                }
            }
            {
                var needSellList = new CoinDao().ListBuySuccessAndNoSellRecord(accountId, coin, 1);
                foreach (var item in needSellList)
                {
                    // 分析是否 大于
                    decimal itemNowOpen = 0;
                    decimal higher = new CoinAnalyze().AnalyzeNeedSell(item.BuyOrderPrice, item.BuyDate, coin, "usdt", out itemNowOpen, res);

                    decimal gaoyuPercentSell = (decimal)1.016;
                    bool needHuitou = true;// 如果很久没有出售过,则要考虑不需要回头
                    gaoyuPercentSell = (decimal)1.016;

                    if (CheckCanSell(item.BuyOrderPrice, higher, itemNowOpen, gaoyuPercentSell, needHuitou))
                    {
                        decimal sellQuantity = item.BuyTotalQuantity * (decimal)0.99;
                        sellQuantity = decimal.Round(sellQuantity, getSellPrecisionNumber(coin));
                        if (coin == "xrp" && sellQuantity < 1)
                        {
                            sellQuantity = 1;
                        }
                        // 出售
                        decimal sellPrice = decimal.Round(itemNowOpen * (decimal)0.985, getPrecisionNumber(coin));
                        ResponseOrder order = new AccountOrder().NewOrderSell(accountId, sellQuantity, sellPrice, null, coin, "usdt");
                        if (order.status != "error")
                        {
                            new CoinDao().ChangeDataWhenSell(item.Id, sellQuantity, sellPrice, JsonConvert.SerializeObject(order), JsonConvert.SerializeObject(flexPointList), order.data);
                            // 下单成功马上去查一次
                            QuerySellDetailAndUpdate(order.data);
                        }
                        else
                        {
                            logger.Error($"出售结果 coin{coin} accountId:{accountId}  出售数量{sellQuantity} itemNowOpen{itemNowOpen} higher{higher} {JsonConvert.SerializeObject(order)}");
                            logger.Error($"出售结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                        }
                        ClearData();
                    }
                }
            }
        }
    }
}
