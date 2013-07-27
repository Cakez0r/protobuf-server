using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.NPCs
{
    public class PostgresNPCRepository : PostgresRepository, INPCRepository
    {
        private IReadOnlyCollection<NPCModel> m_npcCache;
        private IReadOnlyCollection<NPCSpawnModel> m_npcSpawnCache;

        public IEnumerable<NPCModel> GetNPCs()
        {
            if (m_npcCache == null)
            {
                m_npcCache = Function<NPCModel>("GET_NPCs", null).ToList().AsReadOnly();

            }

            return m_npcCache;
        }

        public IEnumerable<NPCSpawnModel> GetNPCSpawns()
        {
            if (m_npcSpawnCache == null)
            {
                m_npcSpawnCache = Function<NPCSpawnModel>("GET_NPCSpawns", null).ToList().AsReadOnly();
            }

            return m_npcSpawnCache;
        }
    }
}
