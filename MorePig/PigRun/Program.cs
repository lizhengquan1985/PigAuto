using log4net;
using log4net.Config;
using Newtonsoft.Json;
using PigAccount;
using PigPlatform;
using PigPlatform.Model;
using PigRunService;
using PigRunService.BeginUtils;
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

            // 初始化
            CoinUtils.Init();
            var symbols = CoinUtils.GetAllCommonSymbols();

            // 初始化kline
            KlineUtils.Begin();

            // 不停的对每个币做操作
            BuyOrSellUtils.Begin();

            // 状态检查
            TradeStateUtils.Begin();

            while (true)
            {
                Console.ReadLine();
            }
        }

        
    }
}
