using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class User : Header
    {
        public string UserId { get; set; }

        public long Balance { get; set; }

        public long OriginBalance { get; set; }

        public bool AutoTrade { get; set; }

        public int AutoTradeCount { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AnalysisType? AnalysisType { get; set; }
    }
}
