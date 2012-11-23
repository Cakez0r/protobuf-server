using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class PlayerStateUpdate
    {
        [ProtoMember(1)]
        public int ID { get; set; }

        [ProtoMember(2)]
        public float X { get; set; }

        [ProtoMember(3)]
        public float Y { get; set; }
        
        [ProtoMember(4)]
        public float Z { get; set; }

        [ProtoMember(5)]
        public float Rot { get; set; }
    }
}
