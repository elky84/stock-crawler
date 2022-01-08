using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class NotificationMulti : EzAspDotNet.Protocols.ResponseHeader
    {
        public List<Common.Notification> Datas { get; set; }
    }
}
