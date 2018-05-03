using PigPlatform;
using PigPlatform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    CoinTrade.Run(symbol);
                }
            });
        }
    }
}
