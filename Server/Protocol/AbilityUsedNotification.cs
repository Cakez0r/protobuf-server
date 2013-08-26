using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class AbilityUsedNotification : Packet
    {
        [ProtoMember(1)]
        public int SourceID { get; set; }

        [ProtoMember(2)]
        public int TargetID { get; set; }

        [ProtoMember(3)]
        public int AbilityID { get; set; }

        [ProtoMember(4)]
        public int Timestamp { get; set; }
    }
}
