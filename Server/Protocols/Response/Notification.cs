namespace Server.Protocols.Response
{
    public class Notification : EzAspDotNet.Protocols.ResponseHeader
    {
        public Common.Notification Data { get; set; }
    }
}
