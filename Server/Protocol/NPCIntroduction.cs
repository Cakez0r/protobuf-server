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
    }
}
