using EzMongoDb.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;

namespace Server.Models
{
    public class MockInvestHistory : MongoDbHeader
    {
        [BsonRepresentation(BsonType.String)]
        public HistoryType Type { get; set; }

        public string UserId { get; set; }

        public string Code { get; set; }

        public int Amount { get; set; }

        public int BuyPrice { get; set; }

        public int? Price { get; set; }

        public DateTime Date { get; set; }

        public MockInvestHistory()
        {
        }

        public MockInvestHistory(HistoryType type, MockInvest mockInvest, int? amount = null)
        {
            this.Type = type;
            this.UserId = mockInvest.UserId;
            this.Code = mockInvest.Code;
            this.Amount = amount ?? mockInvest.Amount;
            this.BuyPrice = mockInvest.BuyPrice;
            this.Price = mockInvest.Price;
            this.Date = mockInvest.Date;
        }
    }
}
