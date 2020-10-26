using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class MockInvestAutoTradeRefresh
    {
        public string UserId { get; set; }

        public DateTime? Date { get; set; } = DateTime.Now.Date;
    }
}
