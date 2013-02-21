using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public abstract class Packet
    {
        [ProtoMember(1)]
        public ushort ID { get; set; }
    }
}
