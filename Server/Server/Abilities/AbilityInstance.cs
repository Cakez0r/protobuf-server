using Data.Abilities;

namespace Server.Abilities
{
    public class AbilityInstance
    {
        public AbilityModel Ability
        {
            get;
            private set; 
        }

        public ITargetable Source
        {
            get;
            private set;
        }

        public ITargetable Target
        {
            get;
            private set;
        }

        public AbilityInstance(ITargetable source, ITargetable target, AbilityModel ability)
        {
            Source = source;
            Target = target;
            Ability = ability;
        }
    }
}
