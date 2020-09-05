using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class Analysis
    {
        public AnalysisType Type { get; set; }

        public List<int> Days { get; set; }

        public List<string> Codes { get; set; }

        public bool All { get; set; }
    }
}
