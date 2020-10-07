using System;
using System.Collections.Generic;

namespace Server.Protocols.Common
{
    public class MockInvestList
    {
        public User User { get; set; }

        public List<MockInvest> MockInvests { get; set; } = new List<MockInvest>();

        public long ValuationBalance { get; set; }

        public double ValuationIncome { get; set; }

        public double InvestedIncome { get; set; }

        public DateTime? Date { get; set; }
    }
}
