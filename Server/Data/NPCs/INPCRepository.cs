using System.Collections.Generic;

namespace Data.NPCs
{
    public interface INPCRepository
    {
        IEnumerable<NPCModel> GetNPCs();
        IEnumerable<NPCSpawnModel> GetNPCSpawns();
        NPCModel GetNPCByID(int npcID);
        IEnumerable<NPCStatModel> GetNPCStats();
        IEnumerable<NPCBehaviourModel> GetNPCBehaviours();
        IEnumerable<NPCBehaviourVarModel> GetNPCBehaviourVars();
        IEnumerable<NPCBehaviourModel> GetNPCBehavioursByNPCID(int npcID);
        IReadOnlyDictionary<int, float> GetNPCStatsByNPCID(int npcID);
        IReadOnlyDictionary<string, string> GetNPCBehaviourVarsByNPCBehaviourID(int npcBehaviourID);
    }
}
