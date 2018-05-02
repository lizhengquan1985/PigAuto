using log4net;
using log4net.Config;
using PigAccount;
using PigPlatform;
using PigPlatform.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            ILog logger = LogManager.GetLogger("program");

            AccountConfig.init("xx");
            PlatformApi api = new PlatformApi(AccountConfig.accessKey, AccountConfig.secretKey);
            var res = api.GetCommonSymbols();
            var res1 = api.GetHistoryKline("ltcusdt", "1min");

            OrderPlaceRequest req = new OrderPlaceRequest();
            req.account_id = AccountConfig.mainAccountId;
            req.amount = "0.000001";
            req.price = "0.9";
            req.source = "api";
            req.symbol = "ethusdt";
            req.type = "buy-limit";
            HBResponse<long> order = api.OrderPlace(req);

            Console.ReadLine();
        }
    }
}
