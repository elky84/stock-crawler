using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class AutoTrade : Header
    {
        public string UserId { get; set; }

        public long Balance { get; set; }

        public string Code { get; set; }

        public AnalysisType AnalysisType { get; set; }

        public AutoTradeType BuyTradeType { get; set; }

        public double BuyCondition { get; set; }

        public AutoTradeType SellTradeType { get; set; }

        public double SellCondition { get; set; }
    }
}
