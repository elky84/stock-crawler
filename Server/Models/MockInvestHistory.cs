using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;

namespace Server.Models
{
    public class MockInvestHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public HistoryType Type { get; set; }

        public string UserId { get; set; }

        public string Code { get; set; }

        public int Amount { get; set; }

        public int BuyPrice { get; set; }

        public int? Price { get; set; }

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
        }
    }
}
