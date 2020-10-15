using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class MockInvestHistory : Header
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public HistoryType Type { get; set; }

        public string UserId { get; set; }

        public string Code { get; set; }

        public int Amount { get; set; }

        public int BuyPrice { get; set; }

        public DateTime Date { get; set; }

        public int? Price { get; set; }

        public int? Income => Price.HasValue ? BuyPrice - Price.Value : (int?)null;

        public int TotalBuyPrice => Amount * BuyPrice;

        public int? TotalPrice => Price.HasValue ? Price.Value * Amount : (int?)null;

        public int? TotalIncome => TotalPrice.HasValue ? TotalPrice.Value - TotalBuyPrice : (int?)null;

        public double? IncomeRate => TotalPrice.HasValue && TotalBuyPrice != 0 ? 100.0 - (double)TotalBuyPrice / (double)TotalPrice.Value * 100.0 : (double?)null;
    }
}