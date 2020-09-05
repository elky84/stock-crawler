using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Server.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        public long Balance { get; set; }

        public Protocols.Common.User ToProtocol()
        {
            return new Protocols.Common.User
            {
                Id = Id,
                UserId = UserId,
                Balance = Balance
            };
        }
    }
}
