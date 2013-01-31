using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol
{
    [ProtoContract]
    public class PlayerIntroduction
    {
        [ProtoMember(1)]
        public string Name { get; set; }
    }
}
