using log4net;
using Newtonsoft.Json;
using PigAccount;
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
            if (flexPointList == null || flexPointList.Count == 0 || (flexPointList.Count == 1 && flexPointList[0].isHigh))
            {
                flexPercent = (decimal)1.035;
                flexPointList = CoinAnalyze.Analyze(historyKlines, out lastLowPrice, out nowPrice, flexPercent);
            }
            if (flexPointList == null || flexPointList.Count == 0 || (flexPointList.Count == 1 && flexPointList[0].isHigh))
            {
                flexPercent = (decimal)1.03;
                flexPointList = CoinAnalyze.Analyze(historyKlines, out lastLowPrice, out nowPrice, flexPercent);
            }
            if (flexPointList == null || flexPointList.Count == 0 || (flexPointList.Count == 1 && flexPointList[0].isHigh))
            {
                flexPercent = (decimal)1.025;
                flexPointList = CoinAnalyze.Analyze(historyKlines, out lastLowPrice, out nowPrice, flexPercent);
            }
            if (flexPointList == null || flexPointList.Count == 0 || (flexPointList.Count == 1 && flexPointList[0].isHigh))
            {
                flexPercent = (decimal)1.02;
                flexPointList = CoinAnalyze.Analyze(historyKlines, out lastLowPrice, out nowPrice, flexPercent);
            }
            if (flexPointList == null || flexPointList.Count == 0 || (flexPointList.Count == 1 && flexPointList[0].isHigh))
            {
                flexPercent = (decimal)1.015;
                flexPointList = CoinAnalyze.Analyze(historyKlines, out lastLowPrice, out nowPrice, flexPercent);
            }
            if (flexPointList.Count == 0 && flexPointList.Count == 0)
            {
                logger.Error($"--------------> 分析{symbol.BaseCurrency}的flexPoint结果数量为0 ");
                return;
            }

            var accountInfo = api.GetAccountBalance(accountId);
            var usdt = accountInfo.data.list.Find(it => it.currency == "usdt");
            decimal recommendAmount = usdt.balance / 600; // TODO 测试阶段，暂定低一些，
            Console.Write($"spot--------> 开始 {symbol.QuoteCurrency}  推荐额度：{decimal.Round(recommendAmount, 2)} ");


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


            // 获取最近的购买记录
            // 购买的要求
            // 1. 最近一次是低点， 并且有上升的迹象。
            // 2. 快速上升的，快速下降情况（如果升的太高， 最一定要回落，或者有5个小时平稳才考虑购入，）
            // 3. 如果flexpoint 小于等于1.02，则只能考虑买少一点。
            // 4. 余额要足够，推荐购买的额度要大于0.3
            // 5. 
            if (!flexPointList[0].isHigh && recommendAmount > (decimal)0.3  &&  !JudgeBuyUtils.IsQuickRise(symbol.QuoteCurrency, historyKlines) && JudgeBuyUtils.CheckCalcMaxhuoluo())
            {
                decimal buyQuantity = recommendAmount / nowPrice;
                buyQuantity = decimal.Round(buyQuantity, GetBuyQuantityPrecisionNumber(coin));
                decimal orderPrice = decimal.Round(nowPrice * (decimal)1.005, getPrecisionNumber(coin));
                ResponseOrder order = new AccountOrder().NewOrderBuy(accountId, buyQuantity, orderPrice, null, symbol.QuoteCurrency, symbol.BaseCurrency);
                if (order.status != "error")
                {
                    new PigMoreDao().CreatePigMore(new PigMore()
                    {
                        Name = symbol.QuoteCurrency,
                        UserName = AccountConfig.userName,
                        BQuantity = buyQuantity,
                        BOrderP = orderPrice,
                        BDate = DateTime.Now,
                        HasSell = false,
                        BOrderResult = JsonConvert.SerializeObject(order),
                        BAnalyze = JsonConvert.SerializeObject(flexPointList),
                        AccountId = accountId,
                        BState = "",
                        BTradeP = 0,
                        BOrderId = order.data,
                        BOrderQ = "",
                        SAnalyze = "",
                        SOrderId = "",
                        SOrderResult = ""
                    });
                    ClearData();
                    // 下单成功马上去查一次
                    QueryDetailAndUpdate(order.data);
                }
                else
                {
                    logger.Error($"下单结果 coin{symbol.QuoteCurrency} accountId:{accountId}  购买数量{buyQuantity} nowOpen{nowPrice} {JsonConvert.SerializeObject(order)}");
                    logger.Error($"下单结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                }
            }

            {
                var needSellList = new PigMoreDao().ListBuySuccessAndNoSellRecord(accountId, coin, 0);
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

                    var canSell = JudgeSellUtils.CheckCanSell(item.BOrderP, higher, itemNowOpen, gaoyuPercentSell, needHuitou);

                    if (canSell)
                    {
                        decimal sellQuantity = item.BQuantity * (decimal)0.99;
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

        private static void QueryDetailAndUpdate(string orderId)
        {
            string orderQuery = "";
            var queryOrder = new AccountOrder().QueryOrder(orderId, out orderQuery);
            if (queryOrder.status == "ok" && queryOrder.data.state == "filled")
            {
                string orderDetail = "";
                var detail = new AccountOrder().QueryDetail(orderId, out orderDetail);
                decimal maxPrice = 0;
                foreach (var item in detail.data)
                {
                    if (maxPrice < item.price)
                    {
                        maxPrice = item.price;
                    }
                }
                if (detail.status == "ok")
                {
                    new CoinDao().UpdateTradeRecordBuySuccess(orderId, maxPrice, orderQuery);
                }
            }
        }

        private static void QuerySellDetailAndUpdate(string orderId)
        {
            string orderQuery = "";
            var queryOrder = new AccountOrder().QueryOrder(orderId, out orderQuery);
            if (queryOrder.status == "ok" && queryOrder.data.state == "filled")
            {
                string orderDetail = "";
                var detail = new AccountOrder().QueryDetail(orderId, out orderDetail);
                decimal minPrice = 99999999;
                foreach (var item in detail.data)
                {
                    if (minPrice > item.price)
                    {
                        minPrice = item.price;
                    }
                }
                // 完成
                new CoinDao().UpdateTradeRecordSellSuccess(orderId, minPrice, orderQuery);
            }
        }
    }
}
