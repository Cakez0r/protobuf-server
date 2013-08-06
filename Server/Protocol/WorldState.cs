using ProtoBuf;
using System.Collections.Generic;

namespace Protocol
{
    [ProtoContract]
    public class WorldState : Packet
    {
        [ProtoMember(1)]
        public int CurrentServerTime { get; set; }

        [ProtoMember(2)]
        public List<PlayerStateUpdate_S2C> PlayerStates { get; set; }
        
        [ProtoMember(3)]
        public List<PlayerIntroduction> PlayerIntroductions { get; set; }

        [ProtoMember(4)]
        public List<NPCStateUpdate> NPCStates { get; set; }

        [ProtoMember(5)]
        public List<NPCIntroduction> NPCIntroductions { get; set; }
    }
}
