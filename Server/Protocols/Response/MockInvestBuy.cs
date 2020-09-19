using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class MockInvestBuy : Header
    {
        public Common.User User { get; set; }

        public Common.MockInvest InvestData { get; set; }

        public DateTime? Date { get; set; }
    }
}
