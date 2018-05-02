﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRunService
{
    public class HistoryKlinePools
    {
        /// <summary>
        /// symbol, 1min, DateTime.Now().toString("yyyyMMddHHmm");
        /// </summary>
        private static ConcurrentDictionary<string, HistoryKlineData> historyKlines = new ConcurrentDictionary<string, HistoryKlineData>();

        public static string GetKey(CommonSymbols symbol, string period)
        {
            return symbol.BaseCurrency + "-" + period;
        }

        public static void Init(string key, List<HistoryKline> data)
        {
            historyKlines.TryAdd(key, new HistoryKlineData()
            {
                Data = data,
                Date = DateTime.Now
            });
        }

        public static HistoryKlineData Get(string key)
        {
            historyKlines.TryGetValue(key, out HistoryKlineData value);
            return value;
        }
    }

    public class HistoryKlineData
    {
        public DateTime Date { get; set; }
        public List<HistoryKline> Data { get; set; }
    }
}
