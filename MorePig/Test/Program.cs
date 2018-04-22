using PigAccount;
using PigPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            AccountConfig.init("lzq");
            PlatformApi api = new PlatformApi(AccountConfig.accessKey, AccountConfig.secretKey);
            var res = api.GetCommonSymbols();
            var res1 = api.GetHistoryKline("ltcusdt", "1min");

            Console.ReadLine();
        }
    }
}
