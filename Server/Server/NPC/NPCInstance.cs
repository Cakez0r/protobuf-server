using Data.NPCs;
using Protocol;
using Server.Abilities;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.NPC
{
    public class NPCInstance : ITargetable
    {
        public int ID { get; private set; }

        public NPCModel NPCModel { get; private set; }
        public NPCSpawnModel NPCSpawnModel { get; private set; }
        public NPCStateUpdate StateUpdate { get; private set; }

        private List<INPCBehaviour> m_behaviours;

        private IReadOnlyDictionary<int, float> m_stats;

        private Fiber m_fiber;

        public Vector2 Position { get; set; }

        public NPCInstance(Fiber fiber, NPCModel npc, NPCSpawnModel npcSpawn, List<INPCBehaviour> behaviours, IReadOnlyDictionary<int, float> stats)
        {
            NPCModel = npc;
            NPCSpawnModel = npcSpawn;
            m_stats = stats;
            m_fiber = fiber;

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

        public UseAbilityResult AcceptAbilityAsSource(AbilityInstance ability)
        {
            return UseAbilityResult.Completed;
        }

        public Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance ability)
        {
            return m_fiber.Enqueue(() =>
            {
                return UseAbilityResult.Completed;
            });
        }
    }
}
