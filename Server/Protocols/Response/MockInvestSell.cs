using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class MockInvestSell : Header
    {
        public Common.User User { get; set; }

        public List<Common.MockInvest> Datas { get; set; }

        public DateTime? Date { get; set; }
    }
}
