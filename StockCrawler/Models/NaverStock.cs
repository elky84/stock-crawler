﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Server.Models
{
    public class NaverStock
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Code { get; set; }

        public int Change { get; set; }

        public int Latest { get; set; }

        public int Start { get; set; }

        public int High { get; set; }

        public int Low { get; set; }

        public int TradeCount { get; set; }

        public DateTime Date { get; set; }
    }
}
