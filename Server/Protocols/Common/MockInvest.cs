using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class MockInvest
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string Code { get; set; }

        public int Amount { get; set; }

        public int BuyPrice { get; set; }

        public DateTime Date { get; set; }

        public int? CurrentPrice { get; set; }

        public int? Income => CurrentPrice.HasValue ? CurrentPrice.Value - BuyPrice : (int?)null;

        public int TotalBuyPrice => Amount * BuyPrice;

        public int? TotalCurrentPrice => CurrentPrice.HasValue ? CurrentPrice.Value * Amount : (int?)null;

        public int? TotalIncome => TotalCurrentPrice.HasValue ? TotalCurrentPrice.Value - TotalBuyPrice : (int?)null;

        public double? IncomeRate => TotalCurrentPrice.HasValue && TotalBuyPrice != 0 ? 100.0 - (double)TotalBuyPrice / (double)TotalCurrentPrice.Value * 100.0 : (double?)null;
    }
}