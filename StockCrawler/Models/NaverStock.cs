using MongoDB.Bson.Serialization.Attributes;
using System;
using WebUtil.Models;

namespace StockCrawler.Models
{
    public class NaverStock : MongoDbHeader
    {
        public string Code { get; set; }

        public int Change { get; set; }

        public int Latest { get; set; }

        public int Start { get; set; }

        public int High { get; set; }

        public int Low { get; set; }

        public int TradeCount { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }
    }
}
