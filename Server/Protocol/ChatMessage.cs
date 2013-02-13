using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class ChatMessage : Packet
    {
        [ProtoMember(1)]
        public string SenderName { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; }
    }
}
