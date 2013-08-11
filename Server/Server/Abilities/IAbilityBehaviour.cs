using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Abilities
{
    public interface IAbilityBehaviour
    {
        Future<UseAbilityResult> Result { get; }

        void Initialise(IReadOnlyDictionary<string, string> vars);

        void Start(AbilityInstance abilityInstance);
        void Update(TimeSpan dt, AbilityInstance abilityInstance);
        void End(AbilityInstance abilityInstance);
    }
}
