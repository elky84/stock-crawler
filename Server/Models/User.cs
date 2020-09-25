using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;
using WebUtil.Models;

namespace Server.Models
{
    public class User : MongoDbHeader
    {
        public string UserId { get; set; }

        public long Balance { get; set; }

        public long OriginBalance { get; set; }

        public bool AutoTrade { get; set; }

        public int AutoTradeCount { get; set; }

        [BsonRepresentation(BsonType.String)]
        public AnalysisType? AnalysisType { get; set; }
    }
}
