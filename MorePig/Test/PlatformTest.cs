using PigPlatform;
using PigPlatform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class PlatformTest
    {
        PlatformApi api = new PlatformApi("", "");

        public void GetAllAccountTest()
        {
            var result = api.GetAllAccount();
        }

        public void OrderPlaceTest()
        {
            var accounts = api.GetAllAccount();
            var spotAccountId = accounts.FirstOrDefault(a => a.Type == "spot" && a.State == "working")?.Id;
            if (spotAccountId <= 0)
                throw new ArgumentException("spot account unavailable");
            OrderPlaceRequest req = new OrderPlaceRequest();
            req.account_id = spotAccountId.ToString();
            req.amount = "0.1";
            req.price = "800";
            req.source = "api";
            req.symbol = "ethusdt";
            req.type = "buy-limit";
            var result = api.OrderPlace(req);
            //Assert.AreEqual(result.Status, "ok");
        }
    }
}
