using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;
using WebUtil.Models;

namespace Server.Models
{
    public class Company : MongoDbHeader
    {
        public string Value { get; set; }

        public string Name { get; set; }

        [BsonRepresentation(BsonType.String)]
        public StockType Type { get; set; }
    }
}
