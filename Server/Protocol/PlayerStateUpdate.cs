using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class PlayerStateUpdate_C2S : Packet
    {
        [ProtoMember(1)]
        public float X { get; set; }

        [ProtoMember(2)]
        public float Y { get; set; }
        
        [ProtoMember(3)]
        public float Rot { get; set; }

        [ProtoMember(4)]
        public int? TargetID { get; set; }
    }

    [ProtoContract]
    public class PlayerStateUpdate_S2C : Packet
    {
        [ProtoMember(1)]
        public float X { get; set; }

        [ProtoMember(2)]
        public float Y { get; set; }

        [ProtoMember(3)]
        public float Rot { get; set; }

        [ProtoMember(4)]
        public int PlayerID { get; set; }

        [ProtoMember(5)]
        public PlayerIntroduction Introduction { get; set; }

        [ProtoMember(6)]
        public int? TargetID { get; set; }

        [ProtoMember(7)]
        public int CurrentHP { get; set; }

        [ProtoMember(8)]
        public int MaxHP { get; set; }
    }
}
