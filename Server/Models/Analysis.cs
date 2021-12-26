using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;
using MongoDbWebUtil.Models;

namespace Server.Models
{
    public class Analysis : MongoDbHeader
    {
        public string Code { get; set; }

        public StockEvaluate StockEvaluate { get; set; }

        public DateTime Date { get; set; }

    }
}
