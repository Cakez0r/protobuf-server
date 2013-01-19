using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class ChatMessage
    {
        [ProtoMember(1)]
        public int SenderID { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; }
    }
}
