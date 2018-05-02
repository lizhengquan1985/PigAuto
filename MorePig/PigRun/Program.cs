using log4net;
using log4net.Config;
using Newtonsoft.Json;
using PigAccount;
using PigPlatform;
using PigPlatform.Model;
using PigRunService;
using PigService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PigRun
{
    class Program
    {
        static ILog logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            ILog logger = LogManager.GetLogger("program");

            logger.Info("----------------------  begin  --------------------------------");

            Console.WriteLine("请输入角色");
            while (true)
            {
                var userName = Console.ReadLine();
                if (userName != "qq" && userName != "xx")
                {
                    continue;
                }
                AccountConfig.init(userName);
                break;
            }

            // 初始化
            CoinUtils.Init();
            Console.WriteLine("交易对："+JsonConvert.SerializeObject(CoinUtils.GetAllCommonSymbols()));
            // TODO 最小购买数量
            PlatformApi.Init(AccountConfig.accessKey, AccountConfig.secretKey);
            PlatformApi api = PlatformApi.GetInstance();
            var symbols = CoinUtils.GetAllCommonSymbols();

            // 定时任务， 不停的获取最新数据， 以供分析使用
            foreach (var symbol in symbols)
            {
                RunHistoryKline(symbol, api);
            }

            // 不停的对每个币做操作
            foreach (var symbol in symbols)
            {
                if (symbol.BaseCurrency != "xrp" && symbol.BaseCurrency != "eos" && symbol.BaseCurrency != "elf")
                {
                    continue;
                }
                RunCoin(symbol, api);
            }

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        CoinTrade.CheckBuyOrSellState(api);
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
                    CoinTrade.Run(symbol, api);
                }
            });
        }

        private static void RunHistoryKline(CommonSymbols symbol, PlatformApi api)
        {
            Task.Run(() =>
            {
                var countSuccess = 0;
                var countError = 0;
                while (true)
                {
                    try
                    {
                        var period = "1min";
                        var klines = api.GetHistoryKline(symbol.BaseCurrency + symbol.QuoteCurrency, period);
                        var key = HistoryKlinePools.GetKey(symbol, period);
                        HistoryKlinePools.Init(key, klines);
                        countSuccess++;
                    }
                    catch (Exception ex)
                    {
                        countError++;
                    }
                    Thread.Sleep(1000);
                    if(countSuccess % 100 == 0 || countError % 20 == 0)
                    {
                        Console.WriteLine($"RunHistoryKline -> Success:{countSuccess},Error:{countError}, {symbol.BaseCurrency}");
                    }
                }
            });
        }

    }
}
