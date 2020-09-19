using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;

namespace Server.Protocols.Common
{
    public class Analysis
    {
        public string Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AnalysisType Type { get; set; }

        public string Code { get; set; }

        public StockEvaluate StockEvaluate { get; set; }

        public DateTime Date { get; set; }
    }
}
