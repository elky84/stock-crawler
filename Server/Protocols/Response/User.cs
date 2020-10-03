using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class User : Header
    {
        public Common.User Data { get; set; }
    }
}
