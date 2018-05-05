using log4net;
using log4net.Config;
using PigAccount;
using PigPlatform;
using PigPlatform.Model;
using PigService;
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
            var list = TradeRecord.SpotLzqRecord();
            var i = "";
            foreach(var orderId in list)
            {
                if(i != "ttt") {
                    i = Console.ReadLine();
                }
                PlatformTest.SearchOrder(orderId);
            }

            //var dt = Utils.GetDateById(1525377780);

            //XmlConfigurator.Configure(new FileInfo("log4net.config"));
            //ILog logger = LogManager.GetLogger("program");

            //var userName = "xx";
            //var accountConfig = AccountConfigUtils.GetAccountConfig(userName);
            //PlatformApi api = PlatformApi.GetInstance(userName);
            //var res = api.GetCommonSymbols();
            //var res1 = api.GetHistoryKline("ltcusdt", "1min");

            //OrderPlaceRequest req = new OrderPlaceRequest();
            //req.account_id = accountConfig.MainAccountId;
            //req.amount = "0.000001";
            //req.price = "0.9";
            //req.source = "api";
            //req.symbol = "ethusdt";
            //req.type = "buy-limit";
            //HBResponse<long> order = api.OrderPlace(req);

            Console.ReadLine();
        }
    }
}
