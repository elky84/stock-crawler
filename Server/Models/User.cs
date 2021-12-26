using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Server.Code;
using System;
using MongoDbWebUtil.Models;

namespace Server.Models
{
    public class User : MongoDbHeader
    {
        public string UserId { get; set; }

        public long Balance { get; set; }

        public long OriginBalance { get; set; }
    }
}
