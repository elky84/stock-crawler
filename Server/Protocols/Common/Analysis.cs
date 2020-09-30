using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;

namespace Server.Protocols.Common
{
    public class Analysis : Header
    {
        public string Code { get; set; }

        public StockEvaluate StockEvaluate { get; set; }

        public DateTime Date { get; set; }
    }
}
