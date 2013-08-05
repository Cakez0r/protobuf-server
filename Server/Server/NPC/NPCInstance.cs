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
        public int ID { get; private set; }

        public NPCModel NPCModel { get; private set; }
        public NPCSpawnModel NPCSpawnModel { get; set; }
        public NPCStateUpdate StateUpdate { get; set; }

        private List<INPCBehaviour> m_behaviours;

        public Vector2 Position
        {
            get;
            set;
        }

        public NPCInstance(NPCModel npc, NPCSpawnModel npcSpawn, List<INPCBehaviour> behaviours)
        {
            NPCModel = npc;
            NPCSpawnModel = npcSpawn;

            Position = new Vector2((float)npcSpawn.X, (float)npcSpawn.Y);
            ID = IDGenerator.GetNextID();

            StateUpdate = new NPCStateUpdate()
            {
                Rotation = npcSpawn.Rotation,
                NPCID = npc.NPCID,
                NPCInstanceID = ID
            };

            m_behaviours = behaviours;
        }

        public void Update(TimeSpan dt)
        {
            foreach (INPCBehaviour behaviour in m_behaviours)
            {
                behaviour.Update(dt, this);
            }

            StateUpdate.X = Position.X;
            StateUpdate.Y = Position.Y;
        }
    }
}
