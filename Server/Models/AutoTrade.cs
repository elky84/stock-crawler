using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;
using WebUtil.Models;

namespace Server.Models
{
    public class AutoTrade : MongoDbHeader
    {
        public string UserId { get; set; }

        public long Balance { get; set; }

        public string Code { get; set; }

        public double BuyCondition { get; set; }

        public double SellCondition { get; set; }
    }
}
