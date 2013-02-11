using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol
{
    [ProtoContract]
    public class TimeSync_C2S : Packet
    {
    }

    [ProtoContract]
    public class TimeSync_S2C : Packet
    {
        [ProtoMember(1)]
        public int Time { get; set; }
    }
}
