using System.Collections.Generic;

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
