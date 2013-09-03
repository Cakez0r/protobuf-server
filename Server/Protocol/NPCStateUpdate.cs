using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class NPCStateUpdate : EntityStateUpdate
    {
        [ProtoMember(1)]
        public int NPCInstanceID { get; set; }
    }
}
