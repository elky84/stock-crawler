using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class StockEvaluate
    {
        public Dictionary<int, int> MovingAverageLines { get; set; } = new Dictionary<int, int>();

        public double TradeCount { get; set; }

        public double BuyStockValue { get; set; }

        public double TransactionPrice { get; set; }

        public bool BuyStockTiming => BuyStockValue > 0;
    }
}
