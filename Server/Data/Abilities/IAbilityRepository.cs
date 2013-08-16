using System.Collections.Generic;

namespace Data.Abilities
{
    public interface IAbilityRepository
    {
        IEnumerable<AbilityModel> GetAbilities();
        AbilityModel GetAbilityByID(int abilityID);
    }
}
