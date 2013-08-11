using Data.NPCs;
using Protocol;
using Server.Abilities;
using Server.Utility;
using System;
using System.Collections.Generic;

namespace Server.NPC
{
    public class NPCInstance : ITargetable
    {
        public int ID { get; private set; }

        public NPCModel NPCModel { get; private set; }
        public NPCSpawnModel NPCSpawnModel { get; set; }
        public NPCStateUpdate StateUpdate { get; set; }

        private List<INPCBehaviour> m_behaviours;

        private IReadOnlyDictionary<int, float> m_stats;

        private Fiber m_fiber;
        ThreadSafeWrapper<ITargetable> m_accessor;

        public Vector2 Position
        {
            get;
            set;
        }

        public int Health
        {
            get;
            set;
        }

        public int MaxHealth
        {
            get;
            set;
        }

        public NPCInstance(Fiber fiber, NPCModel npc, NPCSpawnModel npcSpawn, List<INPCBehaviour> behaviours, IReadOnlyDictionary<int, float> stats)
        {
            NPCModel = npc;
            NPCSpawnModel = npcSpawn;
            m_stats = stats;
            m_fiber = fiber;

            m_accessor = new ThreadSafeWrapper<ITargetable>(this, fiber);

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

        public float GetStatValue(int statID)
        {
            float value = default(float);

            m_stats.TryGetValue(statID, out value);

            return value;
        }

        public Future<UseAbilityResult> RunAbilityMutation(Func<ITargetable, UseAbilityResult> mutator)
        {
            return m_accessor.Transaction<UseAbilityResult>(mutator);
        }
    }
}
