using Data.NPCs;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Server.NPC
{
    public class NPCFactory
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private INPCRepository m_npcRepository;
        private Dictionary<int, Type> m_behaviourTypes;

        public NPCFactory(INPCRepository npcRepository)
        {
            m_npcRepository = npcRepository;
            LoadBehaviourTypes();
        }

        public NPCInstance SpawnNPC(NPCSpawnModel spawn)
        {
            NPCModel npc = m_npcRepository.GetNPCByID(spawn.NPCID);
            List<INPCBehaviour> behaviours = new List<INPCBehaviour>();
            foreach (NPCBehaviourModel behaviourModel in m_npcRepository.GetNPCBehavioursByNPCID(spawn.NPCID))
            {
                INPCBehaviour behaviour = (INPCBehaviour)Activator.CreateInstance(m_behaviourTypes[behaviourModel.NPCBehaviourID]);
                
                IReadOnlyDictionary<string, string> behaviourVars = m_npcRepository.GetNPCBehaviourVarsByNPCBehaviourID(behaviourModel.NPCBehaviourID);
                behaviour.Initialise(behaviourVars);

                behaviours.Add(behaviour);
            }

            NPCInstance npcInstance = new NPCInstance(npc, spawn, behaviours);

            return npcInstance;
        }

        private void LoadBehaviourTypes()
        {
            s_log.Info("Loading NPC Behaviour Types...");
            m_behaviourTypes = new Dictionary<int, Type>();
            Dictionary<string, Type> allAssemblyTypes = Assembly.GetCallingAssembly().GetTypes().ToDictionary(t => t.Name);
            foreach (NPCBehaviourModel behaviourModel in m_npcRepository.GetNPCBehaviours())
            {
                Type behaviourType = default(Type);

                if (allAssemblyTypes.TryGetValue(behaviourModel.NPCBehaviourType, out behaviourType))
                {
                    m_behaviourTypes.Add(behaviourModel.NPCBehaviourID, behaviourType);
                }
                else
                {
                    s_log.Warn("Failed to load NPC Behaviour Type: {0}", behaviourModel.NPCBehaviourType);
                }
            }
        }
    }
}
