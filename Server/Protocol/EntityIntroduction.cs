using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public class EntityIntroduction
    {
        [ProtoMember(1)]
        public int ID { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public int MaxPower { get; set; }

        [ProtoMember(4)]
        public int MaxHealth { get; set; }

        [ProtoMember(5)]
        public byte Level { get; set; }

        [ProtoMember(6)]
        public int ModelID { get; set; }
    }
}
