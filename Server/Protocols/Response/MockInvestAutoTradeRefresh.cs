﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class MockInvestAnalysisAutoTrade : EzAspDotNet.Protocols.ResponseHeader
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AnalysisType Type { get; set; }

        public Common.User User { get; set; }

        public List<Common.AutoTrade> Datas { get; set; }

        public DateTime? Date { get; set; }
    }
}
