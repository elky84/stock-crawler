using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class User
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public long Balance { get; set; }
    }
}
