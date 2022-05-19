using EzMongoDb.Models;
using System;

namespace Server.Models
{
    public class Analysis : MongoDbHeader
    {
        public string Code { get; set; }

        public StockEvaluate StockEvaluate { get; set; }

        public DateTime Date { get; set; }

    }
}
