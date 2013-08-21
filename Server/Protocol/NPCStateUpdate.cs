using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class NPCStateUpdate
    {
        [ProtoMember(1)]
        public int NPCID { get; set; }

        [ProtoMember(2)]
        public float X { get; set; }

        [ProtoMember(3)]
        public float Y { get; set; }

        [ProtoMember(4)]
        public float Rotation { get; set; }

        [ProtoMember(5)]
        public int NPCInstanceID { get; set; }

        [ProtoMember(6)]
        public float VelX { get; set; }

        [ProtoMember(7)]
        public float VelY { get; set; }

        [ProtoMember(8)]
        public int Health { get; set; }

        [ProtoMember(9)]
        public int MaxHealth { get; set; }

        [ProtoMember(10)]
        public float Rot { get; set; }
    }
}
