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

        [ProtoMember(5)]
        public int Time { get; set; }

        [ProtoMember(6)]
        public float VelX { get; set; }

        [ProtoMember(7)]
        public float VelY { get; set; }
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
        public int? TargetID { get; set; }

        [ProtoMember(6)]
        public int CurrentHP { get; set; }

        [ProtoMember(7)]
        public int MaxHP { get; set; }

        [ProtoMember(8)]
        public int Time { get; set; }

        [ProtoMember(9)]
        public float VelX { get; set; }

        [ProtoMember(10)]
        public float VelY { get; set; }
    }
}
