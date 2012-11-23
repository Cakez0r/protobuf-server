using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class TestMessage
    {
        [ProtoMember(1)]
        public int A { get; set; }

        [ProtoMember(2)]
        public int B { get; set; }
    }
}
