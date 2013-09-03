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
    public class PlayerStateUpdate_S2C : EntityStateUpdate
    {
    }
}
