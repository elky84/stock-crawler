﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class MockInvestList : Header
    {
        public List<Common.MockInvestList> Datas { get; set; } = new List<Common.MockInvestList>();
    }
}
