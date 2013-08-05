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
        NPCModel GetNPCByID(int npcID);
        IEnumerable<NPCBehaviourModel> GetNPCBehaviours();
        IEnumerable<NPCBehaviourVarModel> GetNPCBehaviourVars();
        IEnumerable<NPCBehaviourModel> GetNPCBehavioursByNPCID(int npcID);
        IReadOnlyDictionary<string, string> GetNPCBehaviourVarsByNPCBehaviourID(int npcBehaviourID);
    }
}
