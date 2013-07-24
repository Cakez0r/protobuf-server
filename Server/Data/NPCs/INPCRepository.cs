using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.NPCs
{
    public interface INPCRepository
    {
        IEnumerable<NPCModel> GetNPCs();
        IEnumerable<NPCSpawnModel> GetNPCSpawns();
    }
}
