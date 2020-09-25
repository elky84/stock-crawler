using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Server.Models
{
    public class MockInvest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        public string Code { get; set; }

        public int Amount { get; set; }

        public int BuyPrice { get; set; }

        [BsonIgnore]
        public int? Price { get; set; }

        [BsonIgnore]
        public int? Income => Price.HasValue ? Price.Value - BuyPrice : (int?)null;

        [BsonIgnore]
        public int TotalBuyPrice => Amount * BuyPrice;

        [BsonIgnore]
        public int? TotalPrice => Price.HasValue ? Price.Value * Amount : (int?)null;

        [BsonIgnore]
        public int? TotalIncome => TotalPrice.HasValue ? TotalPrice.Value - TotalBuyPrice : (int?)null;

        [BsonIgnore]
        public double? IncomeRate => TotalPrice.HasValue && TotalBuyPrice != 0 ? (double)TotalBuyPrice / (double)TotalPrice.Value * 100.0 : (double?)null;

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Date { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
