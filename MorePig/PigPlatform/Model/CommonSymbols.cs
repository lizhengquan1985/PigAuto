using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigPlatform.Model
{
    public class CommonSymbols
    {
        [JsonProperty(PropertyName = "base-currency")]
        public string BaseCurrency { get; set; }

        [JsonProperty(PropertyName = "quote-currency")]
        public string QuoteCurrency { get; set; }

        [JsonProperty(PropertyName = "price-precision")]
        public string PricePrecision { get; set; }

        [JsonProperty(PropertyName = "amount-precision")]
        public string AmountPrecision { get; set; }

        [JsonProperty(PropertyName = "symbol-partition")]
        public string SymbolPartition { get; set; }
    }
}
