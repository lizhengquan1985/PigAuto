﻿using log4net;
using Newtonsoft.Json;
using PigPlatform;
using PigPlatform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PigRunService
{
    public class KlineUtils
    {
        static ILog logger = LogManager.GetLogger(typeof(KlineUtils));

        public static void Begin()
        {
            logger.Info("----------------------  begin  --------------------------------");
            // 初始化
            CoinUtils.Init();
            var symbols = CoinUtils.GetAllCommonSymbols();

            // 定时任务， 不停的获取最新数据， 以供分析使用
            foreach (var symbol in symbols)
            {
                RunHistoryKline(symbol);
            }
        }

        private static void RunHistoryKline(CommonSymbols symbol)
        {
            Task.Run(() =>
            {
                var countSuccess = 0;
                var countError = 0;
                PlatformApi api = PlatformApi.GetInstance("xx"); // 下面api和角色无关. 随便指定一个xx
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
                    if (countSuccess % 100 == 0 || countError % 20 == 0)
                    {
                        Console.WriteLine($"RunHistoryKline -> Success:{countSuccess},Error:{countError}, {symbol.BaseCurrency}");
                    }
                }
            });
        }
    }
}
