using MongoDB.Bson.Serialization.Attributes;
using System;
using MongoDbWebUtil.Models;

namespace StockCrawler.Entity
{
    public class NaverStockHalt
    {
        public string Code { get; set; }

        public string Reason { get; set; }

        public DateTime Date { get; set; }
    }
}
