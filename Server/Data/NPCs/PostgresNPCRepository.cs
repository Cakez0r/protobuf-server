using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.NPCs
{
    public class PostgresNPCRepository : PostgresRepository, INPCRepository
    {
        public IEnumerable<NPCModel> GetNPCs()
        {
            return Function<NPCModel>("GET_NPCs", null);
        }

        public IEnumerable<NPCSpawnModel> GetNPCSpawns()
        {
            return Function<NPCSpawnModel>("GET_NPCSpawns", null);
        }
    }
}
