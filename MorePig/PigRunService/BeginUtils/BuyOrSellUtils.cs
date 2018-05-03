using PigPlatform;
using PigPlatform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PigRunService.BeginUtils
{
    public class BuyOrSellUtils
    {
        public static void Begin()
        {
            var symbols = CoinUtils.GetAllCommonSymbols();

            foreach (var symbol in symbols)
            {
                if (symbol.BaseCurrency != "xrp" && symbol.BaseCurrency != "eos" && symbol.BaseCurrency != "elf")
                {
                    continue;
                }
                RunCoin(symbol);
            }
        }

        private static void RunCoin(CommonSymbols symbol)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    // 判断kline存不存在, 不存在读取一次.
                    var key = HistoryKlinePools.GetKey(symbol, "1min");
                    var historyKlineData = HistoryKlinePools.Get(key);
                    if (historyKlineData == null || historyKlineData.Data == null || historyKlineData.Data.Count == 0 || historyKlineData.Date < DateTime.Now.AddSeconds(-10))
                    {
                        KlineUtils.InitOneKine(symbol);
                    }

                    CoinTrade.Run(symbol);

                    Thread.Sleep(1000);
                }
            });
        }
    }
}
