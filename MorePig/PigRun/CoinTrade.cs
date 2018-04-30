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

            // 计算是否适合购买
            RunBuy(symbol, api);
            // 计算是否适合出售
            RunSell(flexPercent, flexPointList, symbol, api);
        }

        private static void RunBuy(CommonSymbols symbol, PlatformApi api)
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
            var usdt = accountInfo.Data.list.Find(it => it.currency == "usdt");
            decimal recommendAmount = usdt.balance / 600; // TODO 测试阶段，暂定低一些，
            Console.Write($"spot--------> 开始 {symbol.QuoteCurrency}  推荐额度：{decimal.Round(recommendAmount, 2)} ");

            // 获取最近的购买记录
            // 购买的要求
            // 1. 最近一次是低点， 并且有上升的迹象。
            // 2. 快速上升的，快速下降情况（如果升的太高， 最一定要回落，或者有5个小时平稳才考虑购入，）
            // 3. 如果flexpoint 小于等于1.02，则只能考虑买少一点。
            // 4. 余额要足够，推荐购买的额度要大于0.3
            // 5. 
            if (!flexPointList[0].isHigh && recommendAmount > (decimal)0.3 && !JudgeBuyUtils.IsQuickRise(symbol.QuoteCurrency, historyKlines) && JudgeBuyUtils.CheckCalcMaxhuoluo(historyKlines, api))
            {
                decimal buyQuantity = recommendAmount / nowPrice;

                buyQuantity = decimal.Round(buyQuantity, symbol.AmountPrecision);
                decimal orderPrice = decimal.Round(nowPrice * (decimal)1.005, symbol.PricePrecision);

                OrderPlaceRequest req = new OrderPlaceRequest();
                req.account_id = accountId;
                req.amount = buyQuantity.ToString();
                req.price = orderPrice.ToString();
                req.source = "api";
                req.symbol = "ethusdt";
                req.type = "buy-limit";
                var result = api.OrderPlace(req);
                HBResponse<long> order = api.OrderPlace(req);
                if (order.Status == "ok")
                {
                    new PigMoreDao().CreatePigMore(new PigMore()
                    {
                        Name = symbol.QuoteCurrency,
                        AccountId = accountId,
                        UserName = AccountConfig.userName,
                        FlexPercent = flexPercent,

                        BQuantity = buyQuantity,
                        BOrderP = orderPrice,
                        BDate = DateTime.Now,
                        BOrderResult = JsonConvert.SerializeObject(order),
                        BState = StateConst.PreSubmitted,
                        BTradeP = 0,
                        BOrderId = order.Data,
                        BFlex = "",
                        BMemo = "",
                        BOrderDetail = "",
                        BOrderMatchResults = "",

                        HasSell = false,
                        SOrderId = 0,
                        SOrderResult = "",
                        SDate = DateTime.MinValue,
                        SFlex = "",
                        SMemo = "",
                        SOrderDetail = "",
                        SOrderMatchResults = "",
                        SOrderP = 0,
                        SQuantity = 0,
                        SState = "",
                        STradeP = 0,
                    });
                    // 下单成功马上去查一次
                    QueryBuyDetailAndUpdate(order.Data, api);
                }
                else
                {
                    logger.Error($"下单结果 coin{symbol.QuoteCurrency} accountId:{accountId}  购买数量{buyQuantity} nowOpen{nowPrice} {JsonConvert.SerializeObject(order)}");
                    logger.Error($"下单结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                }
            }
        }

        private static void QueryBuyDetailAndUpdate(long orderId, PlatformApi api)
        {
            var orderDetail = api.QueryOrderDetail(orderId);
            if (orderDetail.Status == "ok" && orderDetail.Data.state == "filled")
            {
                var matchResult = api.QueryOrderMatchResult(orderId);
                decimal maxPrice = 0;
                foreach (var item in matchResult.Data)
                {
                    if (maxPrice < item.price)
                    {
                        maxPrice = item.price;
                    }
                }
                if (matchResult.Status == "ok")
                {
                    new PigMoreDao().UpdatePigMoreBuySuccess(orderId, orderDetail, matchResult, maxPrice);
                }
            }
        }

        private static void RunSell(decimal flexPercent, List<FlexPoint> flexPointList,CommonSymbols symbol, PlatformApi api)
        {
            var accountId = AccountConfig.mainAccountId;
            var key = HistoryKlinePools.GetKey(symbol, "1min");
            var historyKlines = HistoryKlinePools.Get(key);

            if (flexPointList[0].isHigh)
            {
                var needSellPigMoreList = new PigMoreDao().ListPigMore(accountId, symbol.QuoteCurrency, new List<string>() { StateConst.PartialCanceled, StateConst.Filled });

                foreach (var needSellPigMoreItem in needSellPigMoreList)
                {
                    // 分析是否 大于
                    decimal itemNowOpen = 0;
                    decimal higher = JudgeSellUtils.AnalyzeNeedSell(needSellPigMoreItem.BOrderP, needSellPigMoreItem.BDate, symbol.QuoteCurrency, symbol.BaseCurrency, out itemNowOpen, historyKlines);

                    decimal gaoyuPercentSell = (decimal)1.035;

                    bool needHuitou = true;// 如果很久没有出售过,则要考虑不需要回头
                    if (flexPercent < (decimal)1.04)
                    {
                        gaoyuPercentSell = (decimal)1.035;
                        if (flexPointList.Count <= 2 && needSellPigMoreList.Where(it => it.BDate > DateTime.Now.AddDays(-1)).ToList().Count == 0)
                        {
                            // 1天都没有交易. 并且波动比较小. 则不需要回头
                            needHuitou = false;
                        }
                    }

                    var canSell = JudgeSellUtils.CheckCanSell(needSellPigMoreItem.BOrderP, higher, itemNowOpen, gaoyuPercentSell, needHuitou);

                    if (canSell)
                    {
                        decimal sellQuantity = needSellPigMoreItem.BQuantity * (decimal)0.99;
                        sellQuantity = decimal.Round(sellQuantity, symbol.AmountPrecision);
                        if (symbol.BaseCurrency == "xrp" && sellQuantity < 1)
                        {
                            sellQuantity = 1;
                        }
                        // 出售
                        decimal sellPrice = decimal.Round(itemNowOpen * (decimal)0.985, symbol.PricePrecision);
                        OrderPlaceRequest req = new OrderPlaceRequest();
                        req.account_id = accountId;
                        req.amount = sellQuantity.ToString();
                        req.price = sellPrice.ToString();
                        req.source = "api";
                        req.symbol = "ethusdt";
                        req.type = "sell-limit";
                        HBResponse<long> order = api.OrderPlace(req);
                        if (order.Status == "ok")
                        {
                            new PigMoreDao().ChangeDataWhenSell(needSellPigMoreItem.Id, sellQuantity, sellPrice, JsonConvert.SerializeObject(order), JsonConvert.SerializeObject(flexPointList), order.Data);
                            // 下单成功马上去查一次
                            QuerySellDetailAndUpdate(order.Data, api);
                        }
                        else
                        {
                            logger.Error($"出售结果 coin{symbol.QuoteCurrency} accountId:{accountId}  出售数量{sellQuantity} itemNowOpen{itemNowOpen} higher{higher} {JsonConvert.SerializeObject(order)}");
                            logger.Error($"出售结果 分析 {JsonConvert.SerializeObject(flexPointList)}");
                        }
                    }
                }
            }
        }

        private static void QuerySellDetailAndUpdate(long orderId, PlatformApi api)
        {
            var orderDetail = api.QueryOrderDetail(orderId);
            if (orderDetail.Status == "ok" && orderDetail.Data.state == "filled")
            {
                var orderMatchResult = api.QueryOrderMatchResult(orderId);
                decimal minPrice = 99999999;
                foreach (var item in orderMatchResult.Data)
                {
                    if (minPrice > item.price)
                    {
                        minPrice = item.price;
                    }
                }
                // 完成
                new PigMoreDao().UpdateTradeRecordSellSuccess(orderId, orderDetail, orderMatchResult, minPrice);
            }
        }

        public static void CheckBuyOrSellState(PlatformApi api)
        {
            try
            {
                var needChangeBuyStatePigMoreList = new PigMoreDao().ListNeedChangeBuyStatePigMore();
                foreach (var item in needChangeBuyStatePigMoreList)
                {
                    // 如果长时间没有购买成功， 则取消订单。
                    if(item.BDate < DateTime.Now.AddMinutes(-30))
                    {
                        //api.
                    }
                    // TODO
                    QueryBuyDetailAndUpdate(item.BOrderId, api);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

            try
            {
                var needChangeSellStatePigMoreList = new PigMoreDao().ListNeedChangeSellStatePigMore();
                foreach (var item in needChangeSellStatePigMoreList)
                {
                    // 如果长时间没有出售成功， 则取消订单。
                    // TODO
                    QuerySellDetailAndUpdate(item.SOrderId, api);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }
    }
}
