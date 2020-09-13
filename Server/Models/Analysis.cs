using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;

namespace Server.Models
{
    public class Analysis
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public AnalysisType Type { get; set; }

        public string Code { get; set; }

        public StockEvaluate StockEvaluate { get; set; }

        public DateTime Date { get; set; }

        public Protocols.Common.Analysis ToProtocol()
        {
            return new Protocols.Common.Analysis
            {
                Id = Id,
                Code = Code,
                StockEvaluate = StockEvaluate?.ToProtocol(),
                Date = Date
            };
        }
    }
}
