﻿using EzMongoDb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;
using System.Collections.Generic;

namespace Server.Models
{
    public class Notification : MongoDbHeader
    {
        public string Name { get; set; }

        public string HookUrl { get; set; }

        public string Channel { get; set; }

        public string IconUrl { get; set; }

        public InvestType InvestType { get; set; }

        public string Keyword { get; set; }

        public string Prefix { get; set; }

        public string Postfix { get; set; }

        public string FilterKeyword { get; set; }

        public string CrawlingType { get; set; }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<DayOfWeek> FilterDayOfWeeks { get; set; } = new List<DayOfWeek>();

        public string FilterStartTime { get; set; }

        public string FilterEndTime { get; set; }
    }
}
