using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class AbilityUseStarted : Packet
    {
        [ProtoMember(1)]
        public int Result { get; set; }

        [ProtoMember(2)]
        public int FinishTime { get; set; }
    }
}
