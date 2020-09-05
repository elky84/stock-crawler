using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class MockInvestSell
    {
        public string UserId { get; set; }

        public string Id { get; set; }

        public int Amount { get; set; }
    }
}
