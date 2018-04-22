using Newtonsoft.Json;
using PigPlatform;
using PigPlatform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRun
{
    class Program
    {
        static void Main(string[] args)
        {
            CoinUtils.Init();
            PlatformApi api = new PlatformApi();
            var symbols = CoinUtils.GetAllCommonSymbols();
            symbols = symbols.FindAll(it => it.QuoteCurrency == "usdt");

            // 定时任务， 不停的获取最新数据， 以供分析使用
            Task.Run(() =>
            {
                while (true)
                {
                    foreach (var symbol in symbols)
                    {
                        try
                        {
                            var period = "1min";
                            var klines = api.GetHistoryKline(symbol.BaseCurrency + symbol.QuoteCurrency, "1min");
                            var key = symbol.BaseCurrency + "-" + period + "-" + DateTime.Now.ToString("yyyyMMddHHmm");
                            HistoryKlinePools.Init(key, klines);
                            Console.WriteLine(DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"获取基础数据出错。{JsonConvert.SerializeObject(symbol)}");
                        }
                    }
                }
            });

            // 不停的对每个币做操作
            foreach (var symbol in symbols)
            {
                RunCoin(symbol);
            }

            while (true)
            {
                Console.ReadLine();
            }
        }

        public static void RunCoin(CommonSymbols symbol)
        {
            Task.Run(() =>
            {
                // 分析币
            });
        }
    }
}
