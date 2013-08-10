using System.Collections.Generic;
using System.Linq;

namespace Data.Abilities
{
    public class PostgresAbilityRepository : PostgresRepository, IAbilityRepository
    {
        private IReadOnlyDictionary<int, AbilityModel> m_abilityCache;
        private IReadOnlyCollection<AbilityBehaviourModel> m_abilityBehaviourCache;
        private IReadOnlyCollection<AbilityBehaviourVarModel> m_abilityBehaviourVarCache;

        private Dictionary<int, IReadOnlyCollection<AbilityBehaviourModel>> m_abilityBehaviourByAbilityIDCache = new Dictionary<int, IReadOnlyCollection<AbilityBehaviourModel>>();
        private Dictionary<int, IReadOnlyDictionary<string, string>> m_abilityBehaviourVarByAbilityBehaviourIDCache = new Dictionary<int, IReadOnlyDictionary<string, string>>();

        public IEnumerable<AbilityModel> GetAbilities()
        {
            if (m_abilityCache == null)
            {
                m_abilityCache = Function<AbilityModel>("GET_Abilities").ToDictionary(a => a.AbilityID, a => a);
            }

            return m_abilityCache.Values;
        }

        public IEnumerable<AbilityBehaviourModel> GetAbilityBehaviours()
        {
            if (m_abilityBehaviourCache == null)
            {
                m_abilityBehaviourCache = Function<AbilityBehaviourModel>("GET_AbilityBehaviours").ToList().AsReadOnly();
            }

            return m_abilityBehaviourCache;
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

        public IEnumerable<AbilityBehaviourVarModel> GetAbilityBehaviourVars()
        {
            if (m_abilityBehaviourVarCache == null)
            {
                m_abilityBehaviourVarCache = Function<AbilityBehaviourVarModel>("GET_AbilityBehaviourVars").ToList().AsReadOnly();
            }

            return m_abilityBehaviourVarCache;
        }

        public IEnumerable<AbilityBehaviourModel> GetAbilityBehavioursByAbilityID(int abilityID)
        {
            IReadOnlyCollection<AbilityBehaviourModel> behaviours = default(IReadOnlyCollection<AbilityBehaviourModel>);

            if (m_abilityBehaviourCache == null)
            {
                GetAbilityBehaviours();
            }

            if (!m_abilityBehaviourByAbilityIDCache.TryGetValue(abilityID, out behaviours))
            {
                behaviours = m_abilityBehaviourCache.Where(ab => ab.AbilityID == abilityID).ToList().AsReadOnly();
                m_abilityBehaviourByAbilityIDCache.Add(abilityID, behaviours);
            }

            return behaviours;
        }

        public IReadOnlyDictionary<string, string> GetAbilityBehaviourVarsByAbilityBehaviourID(int abilityBehaviourID)
        {
            IReadOnlyDictionary<string, string> behaviourVars = default(IReadOnlyDictionary<string, string>);

            if (m_abilityBehaviourVarCache == null)
            {
                GetAbilityBehaviourVars();
            }

            if (!m_abilityBehaviourVarByAbilityBehaviourIDCache.TryGetValue(abilityBehaviourID, out behaviourVars))
            {
                behaviourVars = m_abilityBehaviourVarCache.Where(abv => abv.AbilityBehaviourID == abilityBehaviourID).ToDictionary(abv => abv.Key, abv => abv.Value);
                m_abilityBehaviourVarByAbilityBehaviourIDCache.Add(abilityBehaviourID, behaviourVars);
            }

            return behaviourVars;
        }
    }
}
