using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class MockInvestSell
    {
        public string UserId { get; set; }

        public bool All { get; set; }


        public List<Common.MockInvestSell> SellList = new List<Common.MockInvestSell>();

        public DateTime Date { get; set; } = DateTime.Now.Date;
    }
}
