using ProtoBuf;
using System.Collections.Generic;

namespace Protocol
{
    [ProtoContract]
    public class StatsChangedNotification : Packet
    {
        [ProtoMember(1)]
        public Dictionary<int, float> NewStats { get; set; }
    }
}
