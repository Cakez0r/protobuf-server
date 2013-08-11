using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Abilities.Behaviours
{
    public class StaticDamageAbilityBehaviour : IAbilityBehaviour
    {
        private int m_damage;

        public Future<UseAbilityResult> Result
        {
            get;
            private set;
        }

        public void Initialise(IReadOnlyDictionary<string, string> vars)
        {
            m_damage = int.Parse(vars["Damage"]);
            Result = new Future<UseAbilityResult>();
        }

        public void Start(AbilityInstance abilityInstance)
        {
        }

        public void Update(TimeSpan dt, AbilityInstance abilityInstance)
        {
        }

        public void End(AbilityInstance abilityInstance)
        {
            Result = abilityInstance.Target.RunAbilityMutation((t) => 
            {
                t.Health -= m_damage;
                return UseAbilityResult.OK;
            });
        }
    }
}
