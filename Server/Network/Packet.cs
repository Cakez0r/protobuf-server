using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    [ProtoContract]
    public class Packet
    {
        [ProtoMember(0)]
        public int ID { get; set; }
    }
}
