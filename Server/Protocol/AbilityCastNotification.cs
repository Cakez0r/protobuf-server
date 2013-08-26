using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class AbilityCastNotification : Packet
    {
        [ProtoMember(1)]
        public int StartTime { get; set; }

        [ProtoMember(2)]
        public int EndTime { get; set; }
    }
}
