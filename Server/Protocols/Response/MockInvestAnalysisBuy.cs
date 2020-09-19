using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class MockInvestAnalysisBuy : Header
    {
        public AnalysisType Type { get; set; }

        public Common.User User { get; set; }

        public List<Common.MockInvest> InvestDatas { get; set; }

        public DateTime? Date { get; set; }
    }
}
