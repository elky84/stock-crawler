using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class User
    {
        public string UserId { get; set; }

        public long Balance { get; set; }

        public long OriginBalance { get; set; }
    }
}
