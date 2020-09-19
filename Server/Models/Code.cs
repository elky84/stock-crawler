﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;

namespace Server.Models
{
    public class Code
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Value { get; set; }

        public string Name { get; set; }

        [BsonRepresentation(BsonType.String)]
        public StockType Type { get; set; }


    }
}
