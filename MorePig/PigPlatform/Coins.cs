using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigPlatform
{
    public class Coin
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 最小操作数量
        /// </summary>
        public decimal MinQuantity { get; set; }
        /// <summary>
        /// 数量精度
        /// </summary>
        public int QuantityPrecision { get; set; }
        /// <summary>
        /// 价格精度, 也就是小数点后面的数字
        /// </summary>
        public int PricePrecision { get; set; }
    }

    public class CoinUtils
    {
        private static Dictionary<string, Coin> coins = new Dictionary<string, Coin>();

        private static void Init()
        {
            Add("", (decimal)1.1, 1, 1);
            Add("", (decimal)1.1, 1, 1);
            Add("", (decimal)1.1, 1, 1);
            Add("", (decimal)1.1, 1, 1);
            Add("", (decimal)1.1, 1, 1);
            Add("", (decimal)1.1, 1, 1);
        }

        private static void Add(string name, decimal minQuantity, int quantityPrecision, int pricePrecision)
        {
            if (coins.ContainsKey(name))
            {
                coins[name] = new Coin { };
            }
            else
            {
                coins.Add(name, new Coin { Name = "a" });
            }
        }

        public static Coin Get(string name)
        {
            if (!coins.ContainsKey(name))
            {
                Init();
            }

            return coins[name];
        }

        public static List<string> GetAllCoins()
        {
            // 总共其实有36个, 后期还会增加
            if (coins.Count < 30)
            {
                Init();
            }
            return coins.Keys.ToList();
        }
    }
}
