using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class User : EzAspDotNet.Protocols.ResponseHeader
    {
        public Common.User Data { get; set; }
    }
}
