using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class Crawling
    {
        public int Page { get; set; }

        public List<string> Codes { get; set; }

        public bool All { get; set; }
    }
}
