using log4net;
using log4net.Config;
using Newtonsoft.Json;
using PigAccount;
using PigPlatform;
using PigPlatform.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            ILog logger = LogManager.GetLogger("program");

            logger.Info("----------------------  begin  --------------------------------");

            Console.WriteLine("请输入角色");
            while(true)
            {
                var userName = Console.ReadLine();
                if(userName != "qq" && userName != "xx")
                {
                    continue;
                }
                AccountConfig.init(userName);
                break;
            }

            // 初始化
            CoinUtils.Init();
            Console.WriteLine(JsonConvert.SerializeObject(CoinUtils.GetAllCommonSymbols()));
            // TODO 最小购买数量
            PlatformApi.Init(AccountConfig.accessKey, AccountConfig.secretKey);
            PlatformApi api = PlatformApi.GetInstance();
            var symbols = CoinUtils.GetAllCommonSymbols();

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
                            var key = HistoryKlinePools.GetKey(symbol, period);
                            HistoryKlinePools.Init(key, klines);
                            //Console.WriteLine(DateTime.Now);
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
                // 分析币 按flex 从大到小排。
                // 计算整体是否跌
                // 计算是否快速升高，以及是否快速降低。
            });
        }
    }
}
