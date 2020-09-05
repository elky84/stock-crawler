using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class MockInvestHistories : Header
    {
        public Common.User User { get; set; }

        public List<Common.MockInvestHistory> InvestHistories { get; set; }
    }
}
