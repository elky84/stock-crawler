﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class MockInvestBuy
    {
        public string UserId { get; set; }

        public string Code { get; set; }

        public int Amount { get; set; }

        public DateTime? Date { get; set; } = DateTime.Now.Date;
    }
}
