namespace Server.Protocols.Response
{
    public class Notification : Header
    {
        public Common.Notification Data { get; set; }
    }
}
