
namespace Data.Abilities
{
    public class AbilityModel
    {
        public static class EAbilityType
        {
            public const int HARM = 0;
            public const int HELP = 1;
        }

        public static class ETargetType
        {
            public const int SINGLE = 0;
            public const int AOE = 2;
        }

        public static class ECastType
        {
            public const int INSTANT = 0;
            public const int CAST = 1;
            public const int CHANNELED = 2;
        }

        public int AbilityID { get; set; }
        public string FriendlyName { get; set; }
        public int AbilityType { get; set; }
        public int TargetType { get; set; }
        public int CastType { get; set; }
        public int TargetHealthDelta { get; set; }
        public int TargetPowerDelta { get; set; }
        public int SourceHealthDelta { get; set; }
        public int SourcePowerDelta { get; set; }
        public int CastTimeMS { get; set; }
        public int Range { get; set; }
        public int ClassAffinity { get; set; }
        public float ThreatModifier { get; set; }
    }
}
