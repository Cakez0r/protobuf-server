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
        public List<EntityIntroduction> EntityIntroductions { get; set; }

        [ProtoMember(4)]
        public int Health { get; set; }

        [ProtoMember(5)]
        public int Power { get; set; }
    }
}
