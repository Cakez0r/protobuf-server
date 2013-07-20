using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class PlayerIntroduction
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public int PlayerID { get; set; }
    }
}
