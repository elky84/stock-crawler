using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class MockInvests : Header
    {
        public Common.User User { get; set; }

        public List<Common.MockInvest> InvestList { get; set; }

        public long ValuationBalance { get; set; }

        public double ValuationIncome { get; set; }
    }
}
