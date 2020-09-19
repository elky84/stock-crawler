using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;

namespace Server.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        public long Balance { get; set; }

        public long OriginBalance { get; set; }

        public bool AutoTrade { get; set; }

        public int AutoTradeCount { get; set; }

        [BsonRepresentation(BsonType.String)]
        public AnalysisType? AnalysisType { get; set; }

    }
}
