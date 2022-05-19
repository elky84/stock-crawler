using EzMongoDb.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;

namespace Server.Models
{
    public class AutoTrade : MongoDbHeader
    {
        public string UserId { get; set; }

        public long Balance { get; set; }

        public string Code { get; set; }

        [BsonRepresentation(BsonType.String)]
        public AnalysisType AnalysisType { get; set; }

        [BsonRepresentation(BsonType.String)]
        public AutoTradeType BuyTradeType { get; set; }

        public double BuyCondition { get; set; }

        [BsonRepresentation(BsonType.String)]
        public AutoTradeType SellTradeType { get; set; }

        public double SellCondition { get; set; }
    }
}
