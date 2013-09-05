using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol
{
    [ProtoContract]
    public class EntityStateUpdate
    {
        [ProtoMember(1)]
        public int ID { get; set; }

        [ProtoMember(2)]
        public ushort X { get; set; }

        [ProtoMember(3)]
        public ushort Y { get; set; }

        [ProtoMember(4)]
        public short VelX { get; set; }

        [ProtoMember(5)]
        public short VelY { get; set; }

        [ProtoMember(6)]
        public byte Rotation { get; set; }

        [ProtoMember(7)]
        public int Health { get; set; }

        [ProtoMember(8)]
        public int Power { get; set; }

        [ProtoMember(9)]
        public int TargetID { get; set; }

        [ProtoMember(10)]
        public int Timestamp { get; set; }

        [ProtoMember(11)]
        public ushort CastingEffect { get; set; }
    }
}
