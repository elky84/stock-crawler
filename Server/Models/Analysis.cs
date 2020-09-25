using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;
using WebUtil.Models;

namespace Server.Models
{
    public class Analysis : MongoDbHeader
    {
        [BsonRepresentation(BsonType.String)]
        public AnalysisType Type { get; set; }

        public string Code { get; set; }

        public StockEvaluate StockEvaluate { get; set; }

        public DateTime Date { get; set; }

    }
}
