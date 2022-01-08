using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class MockInvestList : EzAspDotNet.Protocols.ResponseHeader
    {
        public List<Common.MockInvestList> Datas { get; set; } = new List<Common.MockInvestList>();
    }
}
