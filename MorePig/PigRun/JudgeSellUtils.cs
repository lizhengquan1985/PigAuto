using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigRun
{
    public class JudgeSellUtils
    {
        public static bool CheckCanSell(decimal buyPrice, decimal nearHigherOpen, decimal nowOpen, decimal gaoyuPercentSell = (decimal)1.03, bool needHuitou = true)
        {
            //item.BuyPrice, higher, itemNowOpen
            // if (item.BuyPrice * (decimal)1.05 < higher && itemNowOpen * (decimal)1.005 < higher)
            if (nowOpen < buyPrice * gaoyuPercentSell)
            {
                // 如果不高于 3% 没有意义
                return false;
            }

            if (nowOpen * (decimal)1.005 < nearHigherOpen && needHuitou)
            {
                // 表示回头趋势， 暂时定为 0.5% 就有回头趋势
                return true;
            }

            if (nowOpen * (decimal)1.001 < nearHigherOpen && !needHuitou)
            {
                return true;
            }

            return false;
        }
    }
}
