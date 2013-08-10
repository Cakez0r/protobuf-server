using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Abilities
{
    public interface IAbilityRepository
    {
        IEnumerable<AbilityModel> GetAbilities();
        IEnumerable<AbilityBehaviourModel> GetAbilityBehaviours();
        AbilityModel GetAbilityByID(int abilityID);
        IEnumerable<AbilityBehaviourVarModel> GetAbilityBehaviourVars();
        IEnumerable<AbilityBehaviourModel> GetAbilityBehavioursByAbilityID(int abilityID);
        IReadOnlyDictionary<string, string> GetNPCBehaviourVarsByNPCBehaviourID(int abilityBehaviourID);
    }
}
