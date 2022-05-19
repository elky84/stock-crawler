using EzMongoDb.Models;

namespace Server.Models
{
    public class User : MongoDbHeader
    {
        public string UserId { get; set; }

        public long Balance { get; set; }

        public long OriginBalance { get; set; }
    }
}
