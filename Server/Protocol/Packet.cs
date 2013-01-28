using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class Packet
    {
        [ProtoMember(1)]
        public ushort ID { get; set; }
    }
}
