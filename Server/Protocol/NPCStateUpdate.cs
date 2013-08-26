﻿using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class NPCStateUpdate
    {
        [ProtoMember(1)]
        public int NPCID { get; set; }

        [ProtoMember(2)]
        public ushort X { get; set; }

        [ProtoMember(3)]
        public ushort Y { get; set; }

        [ProtoMember(4)]
        public byte Rot { get; set; }

        [ProtoMember(5)]
        public int NPCInstanceID { get; set; }

        [ProtoMember(6)]
        public short VelX { get; set; }

        [ProtoMember(7)]
        public short VelY { get; set; }

        [ProtoMember(8)]
        public int Health { get; set; }

        [ProtoMember(9)]
        public int Power { get; set; }

        [ProtoMember(10)]
        public int TargetID { get; set; }

        [ProtoMember(11)]
        public int Time { get; set; }

        [ProtoMember(12)]
        public int CastingEffect { get; set; }
    }
}
