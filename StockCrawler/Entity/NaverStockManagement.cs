using MongoDB.Bson.Serialization.Attributes;
using System;
using WebUtil.Models;

namespace StockCrawler.Entity
{
    public class NaverStockManagement
    {
        public string Code { get; set; }

        public int Latest { get; set; }

        public int Change { get; set; }

        public double ChangeRate { get; set; }

        public int TradeCount { get; set; }

        public string Reason { get; set; }

        public DateTime Date { get; set; }
    }
}
