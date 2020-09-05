using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class Analysis
    {
        public string Id { get; set; }

        public AnalysisType Type { get; set; }

        public string Code { get; set; }

        public StockEvaluate StockEvaluate { get; set; }

        public DateTime Date { get; set; }
    }
}
