﻿using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class MockInvestAnalysisBuy
    {
        public string UserId { get; set; }

        public AnalysisType Type { get; set; }

        public int Count { get; set; }

        public int TotalPrice { get; set; }
    }
}
