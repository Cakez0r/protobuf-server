using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class PlayerStateUpdate_C2S : Packet
    {
        [ProtoMember(1)]
        public ushort X { get; set; }

        [ProtoMember(2)]
        public ushort Y { get; set; }
        
        [ProtoMember(3)]
        public byte Rot { get; set; }

        [ProtoMember(4)]
        public int TargetID { get; set; }

        [ProtoMember(5)]
        public int Time { get; set; }

        [ProtoMember(6)]
        public short VelX { get; set; }

        [ProtoMember(7)]
        public short VelY { get; set; }
    }

    [ProtoContract]
    public class PlayerStateUpdate_S2C : Packet
    {
        [ProtoMember(1)]
        public ushort X { get; set; }

        [ProtoMember(2)]
        public ushort Y { get; set; }

        [ProtoMember(3)]
        public byte Rot { get; set; }

        [ProtoMember(4)]
        public int PlayerID { get; set; }

        [ProtoMember(5)]
        public int TargetID { get; set; }

        [ProtoMember(6)]
        public ushort Health { get; set; }

        [ProtoMember(7)]
        public int Time { get; set; }

        [ProtoMember(8)]
        public short VelX { get; set; }

        [ProtoMember(9)]
        public short VelY { get; set; }

        [ProtoMember(10)]
        public ushort Power { get; set; }

        [ProtoMember(11)]
        public ushort CastingEffect { get; set; }
    }
}
