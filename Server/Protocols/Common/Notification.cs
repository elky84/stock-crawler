using Server.Code;

namespace Server.Protocols.Common
{
    public class Notification : Header
    {
        public string Name { get; set; }

        public string HookUrl { get; set; }

        public string Channel { get; set; }

        public string IconUrl { get; set; }

        public InvestType InvestType { get; set; }

    }
}
