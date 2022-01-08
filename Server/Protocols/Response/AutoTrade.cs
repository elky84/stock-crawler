using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class AutoTrade : EzAspDotNet.Protocols.ResponseHeader
    {
        public List<Common.AutoTrade> Datas { get; set; }
    }
}
