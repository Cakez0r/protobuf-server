using System.Collections.Generic;
using System.Linq;

namespace Data.NPCs
{
    public class PostgresNPCRepository : PostgresRepository, INPCRepository
    {
        private Dictionary<int, NPCModel> m_npcCache;

        private IReadOnlyCollection<NPCBehaviourModel> m_npcBehaviourCache;
        private IReadOnlyCollection<NPCBehaviourVarModel> m_npcBehaviourVarCache;
        private IReadOnlyCollection<NPCSpawnModel> m_npcSpawnCache;
        private IReadOnlyCollection<NPCStatModel> m_npcStatCache;

        private Dictionary<int, IReadOnlyCollection<NPCBehaviourModel>> m_npcBehaviourByNPCIDCache = new Dictionary<int,IReadOnlyCollection<NPCBehaviourModel>>();
        private Dictionary<int, IReadOnlyDictionary<string, string>> m_npcBehaviourVarByNPCBehaviourIDCache = new Dictionary<int,IReadOnlyDictionary<string, string>>();
        private Dictionary<int, IReadOnlyCollection<NPCStatModel>> m_npcStatByNPCIDCache = new Dictionary<int, IReadOnlyCollection<NPCStatModel>>();


        public IEnumerable<NPCModel> GetNPCs()
        {
            if (m_npcCache == null)
            {
                m_npcCache = Function<NPCModel>("GET_NPCs").ToDictionary(n => n.NPCID);
            }

            return m_npcCache.Values;
        }

        public IEnumerable<NPCSpawnModel> GetNPCSpawns()
        {
            if (m_npcSpawnCache == null)
            {
                m_npcSpawnCache = Function<NPCSpawnModel>("GET_NPCSpawns").ToList().AsReadOnly();
            }

            return m_npcSpawnCache;
        }

        public NPCModel GetNPCByID(int npcID)
        {
            if (m_npcCache == null)
            {
                GetNPCs();
            }

            NPCModel npc = default(NPCModel);

            m_npcCache.TryGetValue(npcID, out npc);

            return npc;
        }

        public IEnumerable<NPCBehaviourModel> GetNPCBehaviours()
        {
            if (m_npcBehaviourCache == null)
            {
                m_npcBehaviourCache = Function<NPCBehaviourModel>("GET_NPCBehaviours").ToList().AsReadOnly();
            }

            return m_npcBehaviourCache;
        }

        public IEnumerable<NPCBehaviourVarModel> GetNPCBehaviourVars()
        {
            if (m_npcBehaviourVarCache == null)
            {
                m_npcBehaviourVarCache = Function<NPCBehaviourVarModel>("GET_NPCBehaviourVars").ToList().AsReadOnly();
            }

            return m_npcBehaviourVarCache;
        }

        public IEnumerable<NPCBehaviourModel> GetNPCBehavioursByNPCID(int npcID)
        {
            IReadOnlyCollection<NPCBehaviourModel> behaviours = default(IReadOnlyCollection<NPCBehaviourModel>);

            if (m_npcBehaviourCache == null)
            {
                GetNPCBehaviours();
            }

            if (!m_npcBehaviourByNPCIDCache.TryGetValue(npcID, out behaviours))
            {
                behaviours = m_npcBehaviourCache.Where(nb => nb.NPCID == npcID).ToList().AsReadOnly();
                m_npcBehaviourByNPCIDCache.Add(npcID, behaviours);
            }

            return behaviours;
        }

        public IReadOnlyDictionary<string, string> GetNPCBehaviourVarsByNPCBehaviourID(int npcBehaviourID)
        {
            IReadOnlyDictionary<string, string> behaviourVars = default(IReadOnlyDictionary<string, string>);

            if (m_npcBehaviourVarCache == null)
            {
                GetNPCBehaviourVars();
            }

            if (!m_npcBehaviourVarByNPCBehaviourIDCache.TryGetValue(npcBehaviourID, out behaviourVars))
            {
                behaviourVars = m_npcBehaviourVarCache.Where(nbv => nbv.NPCBehaviourID == npcBehaviourID).ToDictionary(nbv => nbv.Key, b => b.Value);
                m_npcBehaviourVarByNPCBehaviourIDCache.Add(npcBehaviourID, behaviourVars);
            }

            return behaviourVars;
        }

        public IEnumerable<NPCStatModel> GetNPCStats()
        {
            if (m_npcStatCache == null)
            {
                m_npcStatCache = Function<NPCStatModel>("GET_NPCStats").ToList().AsReadOnly();
            }

            return m_npcStatCache;
        }

        public IEnumerable<NPCStatModel> GetNPCStatsByNPCID(int npcID)
        {
            IReadOnlyCollection<NPCStatModel> npcStats = default(IReadOnlyCollection<NPCStatModel>);

            if (m_npcStatCache == null)
            {
                GetNPCStats();
            }

            if (!m_npcStatByNPCIDCache.TryGetValue(npcID, out npcStats))
            {
                npcStats = m_npcStatCache.Where(stat => stat.NPCID == npcID).ToList().AsReadOnly();
                m_npcStatByNPCIDCache.Add(npcID, npcStats);
            }

            return npcStats;
        }
    }
}
