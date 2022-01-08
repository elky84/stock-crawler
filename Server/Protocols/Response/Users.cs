using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class Users : EzAspDotNet.Protocols.ResponseHeader
    {
        public List<Common.User> Datas { get; set; }
    }
}
