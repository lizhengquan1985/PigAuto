using log4net;
using Newtonsoft.Json;
using PigAccount;
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
        static ILog logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            logger.Info("----------------------  begin  --------------------------------");
            Console.WriteLine("请输入角色");
            var userName = Console.ReadLine();
            while(userName != "qq" || userName != "xx")
            {
                userName = Console.ReadLine();
                AccountConfig.init(userName);
            }

            // 初始化
            CoinUtils.Init();
            Console.WriteLine(JsonConvert.SerializeObject(CoinUtils.GetAllCommonSymbols()));

            PlatformApi api = new PlatformApi();
            var symbols = CoinUtils.GetAllCommonSymbols();
            symbols = symbols.FindAll(it => it.BaseCurrency == "usdt");

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
                            var klines = api.GetHistoryKline(symbol.QuoteCurrency + symbol.BaseCurrency, "1min");
                            var key = HistoryKlinePools.GetKey(symbol, period);
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
                RunCoin(symbol, api);
            }

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {

                        CoinTrade.CheckBuyOrSellState();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("查看购买以及出售结果" + ex.Message, ex);
                    }
                }
            });

            while (true)
            {
                Console.ReadLine();
            }
        }

        private static void RunCoin(CommonSymbols symbol, PlatformApi api)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        CoinTrade.Run(symbol, api);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message, ex);
                    }
                }
                // 分析币 按flex 从大到小排。
                // 计算整体是否跌
                // 计算是否快速升高，以及是否快速降低。
            });
        }
    }
}
