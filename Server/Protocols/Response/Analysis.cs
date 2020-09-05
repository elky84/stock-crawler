using Server.Code;
using Server.Protocols.Common;
using System.Collections.Generic;

namespace Server.Protocols.Response
{
    public class Analysis : Header
    {
        public AnalysisType Type { get; set; }

        public List<Common.Analysis> AnalysisDatas { get; set; }
    }
}
