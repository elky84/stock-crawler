﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class MockInvestHistories : EzAspDotNet.Protocols.ResponseHeader
    {
        public Common.User User { get; set; }

        public List<Common.MockInvestHistory> Datas { get; set; }
    }
}
