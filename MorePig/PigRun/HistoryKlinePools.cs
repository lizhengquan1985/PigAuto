using PigPlatform.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRun
{
    public class HistoryKlinePools
    {
        /// <summary>
        /// symbol, 1min, DateTime.Now().toString("yyyyMMddHHmm");
        /// </summary>
        private static ConcurrentDictionary<string, List<HistoryKline>> historyKlines = new ConcurrentDictionary<string, List<HistoryKline>>();

        public static string GetKey(CommonSymbols symbol, string period)
        {
            return symbol.BaseCurrency + "-" + period + "-" + DateTime.Now.ToString("yyyyMMddHHmm");
        }

        public static void Init(string key, List<HistoryKline> data)
        {
            historyKlines.TryAdd(key, data);
        }

        public static List<HistoryKline> Get(string key)
        {
            historyKlines.TryGetValue(key, out List<HistoryKline> value);
            return value;
        }
    }
}
