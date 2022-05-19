using EzMongoDb.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;

namespace Server.Models
{
    public class Company : MongoDbHeader
    {
        public string Code { get; set; }

        public string Name { get; set; }

        [BsonRepresentation(BsonType.String)]
        public StockType Type { get; set; }

        [BsonRepresentation(BsonType.String)]
        public AlertType AlertType { get; set; }
    }
}
