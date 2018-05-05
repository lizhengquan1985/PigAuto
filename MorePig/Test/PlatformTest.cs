using PigAccount;
using PigPlatform;
using PigPlatform.Model;
using PigService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class PlatformTest
    {

        public void GetAllAccountTest()
        {
            PlatformApi api = PlatformApi.GetInstance("xx");
            var result = api.GetAllAccount();
        }

        public void OrderPlaceTest()
        {
            PlatformApi api = PlatformApi.GetInstance("xx");
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

        public static void SearchOrder(long orderId)
        {
            var userName = "xx";
            AccountConfig account = AccountConfigUtils.GetAccountConfig(userName);

            PlatformApi api = PlatformApi.GetInstance(userName);

            var orderDetail = api.QueryOrderDetail(orderId);
            Console.WriteLine(orderDetail.Status);
            Console.WriteLine(orderDetail.Data.state);
            if (orderDetail.Status == "ok" && orderDetail.Data.state == "filled")
            {
                Console.WriteLine(orderDetail.Data.price);
                Console.WriteLine(orderDetail.Data.id);
                Console.WriteLine(orderDetail.Data.symbol.Replace("usdt", ""));
                Console.WriteLine(orderDetail.Data.amount);

                if(new PigMoreDao().GetByBOrderId(orderId) != null)
                {
                    Console.WriteLine("订单存在");
                    return;
                }

                new PigMoreDao().CreatePigMore(new PigMore()
                {
                    Name = orderDetail.Data.symbol.Replace("usdt",""),
                    AccountId = account.MainAccountId,
                    UserName = account.UserName,
                    FlexPercent = (decimal)1.04,

                    BQuantity = orderDetail.Data.amount,
                    BOrderP = orderDetail.Data.price,
                    BDate = DateTime.Now,
                    BOrderResult = "",
                    BState = StateConst.Submitting,
                    BTradeP = 0,
                    BOrderId = orderId,
                    BFlex = "",
                    BMemo = "",
                    BOrderDetail = "",
                    BOrderMatchResults = "",

                    SOrderId = 0,
                    SOrderResult = "",
                    SDate = DateTime.MinValue,
                    SFlex = "",
                    SMemo = "",
                    SOrderDetail = "",
                    SOrderMatchResults = "",
                    SOrderP = 0,
                    SQuantity = 0,
                    SState = "",
                    STradeP = 0,
                });
            }

            
        }
    }
}
