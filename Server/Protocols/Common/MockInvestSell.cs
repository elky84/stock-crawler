using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class MockInvestSell : Header
    {
        public int Amount { get; set; }
    }
}