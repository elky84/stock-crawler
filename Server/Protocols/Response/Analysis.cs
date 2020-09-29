using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using Server.Protocols.Common;
using System.Collections.Generic;

namespace Server.Protocols.Response
{
    public class Analysis : Header
    {
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<AnalysisType> Types { get; set; }

        public List<Common.Analysis> AnalysisDatas { get; set; }
    }
}
