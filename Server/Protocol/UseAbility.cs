using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    [ProtoContract]
    public class UseAbility_C2S : Packet
    {
        [ProtoMember(1)]
        public int TargetID { get; set; }

        [ProtoMember(2)]
        public int AbilityID { get; set; }
    }

    [ProtoContract]
    public class UseAbility_S2C : Packet
    {
        [ProtoMember(1)]
        public int Result { get; set; }
    }
}
