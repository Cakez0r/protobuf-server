using System.Collections.Generic;
using System.Linq;

namespace Data.Abilities
{
    public class PostgresAbilityRepository : PostgresRepository, IAbilityRepository
    {
        private IReadOnlyDictionary<int, AbilityModel> m_abilityCache;

        public IEnumerable<AbilityModel> GetAbilities()
        {
            if (m_abilityCache == null)
            {
                m_abilityCache = Function<AbilityModel>("GET_Abilities").ToDictionary(a => a.AbilityID, a => a);
            }

            return m_abilityCache.Values;
        }

        public AbilityModel GetAbilityByID(int abilityID)
        {
            if (m_abilityCache == null)
            {
                GetAbilities();
            }

            AbilityModel ability = default(AbilityModel);

            m_abilityCache.TryGetValue(abilityID, out ability);

            return ability;
        }
    }
}
