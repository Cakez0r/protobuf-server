using Data.Abilities;
using NLog;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Server.Abilities
{
    public class AbilityCoordinator
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static readonly Future<UseAbilityResult> s_abilityFailedResult = new Future<UseAbilityResult>();

        private IAbilityRepository m_abilityRepository;
        private Dictionary<int, Type> m_behaviourTypes;

        private List<AbilityInstance> m_activeAbilities = new List<AbilityInstance>();

        static AbilityCoordinator()
        {
            s_abilityFailedResult.SetResult(UseAbilityResult.Failed);
        }

        public AbilityCoordinator(IAbilityRepository abilityRepository)
        {
            m_abilityRepository = abilityRepository;
            LoadBehaviourTypes();
        }

        public void Update(TimeSpan dt)
        {
            foreach (AbilityInstance ability in m_activeAbilities)
            {
                ability.Update(dt);
            }
        }

        public Future<UseAbilityResult> UseAbility(int abilityID, ITargetable source, ITargetable target)
        {
            AbilityModel ability = m_abilityRepository.GetAbilityByID(abilityID);
            if (ability != null)
            {
                List<IAbilityBehaviour> behaviours = new List<IAbilityBehaviour>();
                foreach (AbilityBehaviourModel behaviourModel in m_abilityRepository.GetAbilityBehavioursByAbilityID(ability.AbilityID))
                {
                    IAbilityBehaviour behaviour = (IAbilityBehaviour)Activator.CreateInstance(m_behaviourTypes[behaviourModel.AbilityBehaviourID]);

                    IReadOnlyDictionary<string, string> vars = m_abilityRepository.GetAbilityBehaviourVarsByAbilityBehaviourID(behaviourModel.AbilityBehaviourID);
                    behaviour.Initialise(vars);

                    behaviours.Add(behaviour);
                }

                AbilityInstance abilityInstance = new AbilityInstance(source, target, behaviours);

                m_activeAbilities.Add(abilityInstance);

                return abilityInstance.Result;
            }
            else
            {
                s_log.Warn("[{0}] Tried to use an ability which doesn't exist - {1}.", source.ID, abilityID);
            }

            return s_abilityFailedResult;
        }

        private void LoadBehaviourTypes()
        {
            s_log.Info("Loading Ability Behaviour Types...");
            m_behaviourTypes = new Dictionary<int, Type>();
            Dictionary<string, Type> allAssemblyTypes = Assembly.GetCallingAssembly().GetTypes().ToDictionary(t => t.Name);
            foreach (AbilityBehaviourModel behaviourModel in m_abilityRepository.GetAbilityBehaviours())
            {
                Type behaviourType = default(Type);

                if (allAssemblyTypes.TryGetValue(behaviourModel.BehaviourType, out behaviourType))
                {
                    m_behaviourTypes.Add(behaviourModel.AbilityBehaviourID, behaviourType);
                }
                else
                {
                    s_log.Warn("Failed to load Ability Behaviour Type: {0}", behaviourModel.BehaviourType);
                }
            }
        }
    }
}
