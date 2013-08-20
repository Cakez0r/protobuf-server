using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class Warp : Packet
    {
        [ProtoMember(1)]
        public int ZoneID { get; set; }

        [ProtoMember(2)]
        public float X { get; set; }

        [ProtoMember(3)]
        public float Y { get; set; }
    }
}
