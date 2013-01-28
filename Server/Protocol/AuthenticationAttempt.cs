using ProtoBuf;

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
            BadLogin = 2
        }

        [ProtoMember(1)]
        public ResponseCode Result { get; set; }
    }
}
