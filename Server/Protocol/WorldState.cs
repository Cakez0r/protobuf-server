using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Protocol
{
    [ProtoContract]
    public class WorldState : Packet
    {
        [ProtoMember(1)]
        public int CurrentServerTime { get; set; }


        [ProtoMember(2)]
        public IEnumerable<EntityStateUpdate> EntityStates { get; set; }

        [ProtoMember(3)]
        public List<PlayerIntroduction> PlayerIntroductions { get; set; }

        [ProtoMember(4)]
        public List<NPCIntroduction> NPCIntroductions { get; set; }

        [ProtoMember(5)]
        public int Health { get; set; }

        [ProtoMember(6)]
        public int MaxHealth { get; set; }

        [ProtoMember(7)]
        public int Power { get; set; }

        [ProtoMember(8)]
        public int MaxPower { get; set; }

        [ProtoMember(9)]
        public float XP { get; set; }
    }
}
