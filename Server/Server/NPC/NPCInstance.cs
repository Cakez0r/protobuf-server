using Data.NPCs;
using Protocol;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.NPC
{
    public class NPCInstance
    {
        public NPCModel NPCModel { get; private set; }
        public NPCSpawnModel NPCSpawnModel { get; set; }
        public NPCStateUpdate StateUpdate { get; set; }

        public NPCInstance(NPCModel npc, NPCSpawnModel npcSpawn)
        {
            NPCModel = npc;
            NPCSpawnModel = npcSpawn;

            StateUpdate = new NPCStateUpdate()
            {
                X = (float)npcSpawn.X,
                Y = (float)npcSpawn.Y,
                Rotation = npcSpawn.Rotation,
                NPCID = npc.NPCID,
                NPCInstanceID = IDGenerator.GetNextID()
            };
        }
    }
}
