using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class PlayerIntroduction
    {
        [ProtoMember(1)]
        public string Name { get; set; }
    }
}
