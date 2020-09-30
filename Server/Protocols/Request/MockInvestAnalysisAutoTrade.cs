using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class MockInvestAnalysisAutoTrade
    {
        public string UserId { get; set; }

        public DateTime? Date { get; set; } = DateTime.Now.Date;

        public AnalysisType Type { get; set; }

        public int Count { get; set; }

        public double BuyCondition { get; set; }

        public double SellCondition { get; set; }
    }
}
