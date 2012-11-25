using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class PlayerStateUpdate_C2S
    {
        [ProtoMember(1)]
        public float X { get; set; }

        [ProtoMember(2)]
        public float Y { get; set; }
        
        [ProtoMember(3)]
        public float Rot { get; set; }
    }

    [ProtoContract]
    public class PlayerStateUpdate_S2C
    {
        [ProtoMember(1)]
        public float X { get; set; }

        [ProtoMember(2)]
        public float Y { get; set; }

        [ProtoMember(3)]
        public float Rot { get; set; }

        [ProtoMember(4)]
        public int ID { get; set; }
    }
}
