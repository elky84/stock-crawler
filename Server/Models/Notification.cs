using EzAspDotNet.Models;
using Server.Code;

namespace Server.Models
{
    public class Notification : MongoDbHeader
    {
        public string Name { get; set; }

        public string HookUrl { get; set; }

        public string Channel { get; set; }

        public string IconUrl { get; set; }

        public InvestType InvestType { get; set; }
    }
}
