using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class AutoTrade : Header
    {
        public List<Common.AutoTrade> AutoTradeDatas { get; set; }
    }
}
