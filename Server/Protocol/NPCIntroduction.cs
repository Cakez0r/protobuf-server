using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class NPCIntroduction
    {
        [ProtoMember(1)]
        public int NPCID { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public string Model { get; set; }

        [ProtoMember(4)]
        public float Scale { get; set; }

        [ProtoMember(5)]
        public int MaxHealth { get; set; }

        [ProtoMember(6)]
        public int MaxPower { get; set; }

        [ProtoMember(7)]
        public byte Level { get; set; }
    }
}
