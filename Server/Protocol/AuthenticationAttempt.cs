using ProtoBuf;
using System.Collections.Generic;

namespace Protocol
{
    [ProtoContract]
    public class AuthenticationAttempt_C2S : Packet
    {
        [ProtoMember(1)]
        public string Username { get; set; }

        [ProtoMember(2)]
        public string Password { get; set; }
    }

    [ProtoContract]
    public class AuthenticationAttempt_S2C : Packet
    {
        public enum ResponseCode
        {
            Error = 0,
            OK = 1,
            BadLogin = 2,
            AlreadyLoggedIn = 3
        }

        [ProtoMember(1)]
        public ResponseCode Result { get; set; }

        [ProtoMember(2)]
        public int PlayerID { get; set; }

        [ProtoMember(3)]
        public int ZoneID { get; set; }

        [ProtoMember(4)]
        public float X { get; set; }

        [ProtoMember(5)]
        public float Y { get; set; }

        [ProtoMember(6)]
        public Dictionary<int, float> Stats { get; set; }
    }
}
