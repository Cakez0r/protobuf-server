using Data.NPCs;
using NLog;
using Protocol;
using Server.Abilities;
using Server.Gameplay;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.NPC
{
    public class NPCInstance : ITargetable
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        public int ID { get; private set; }

        public NPCModel NPCModel { get; private set; }
        public NPCSpawnModel NPCSpawnModel { get; private set; }
        public NPCStateUpdate StateUpdate { get; private set; }

        private List<INPCBehaviour> m_behaviours;

        private IReadOnlyDictionary<StatType, float> m_stats;

        private Fiber m_fiber;

        public Vector2 Position { get; set; }

        public string Name
        {
            get { return string.Format("[NPC {0} {1}]", NPCModel.Name, ID); }
        }

        public Vector2 Velocity
        {
            get;
            set;
        }

        public int Health
        {
            get;
            private set;
        }

        public int MaxHealth
        {
            get;
            private set;
        }

        public float Rotation
        {
            get;
            set;
        }

        public bool Dead
        {
            get;
            private set;
        }

        public NPCInstance(Fiber fiber, NPCModel npc, NPCSpawnModel npcSpawn, List<INPCBehaviour> behaviours, IReadOnlyDictionary<StatType, float> stats)
        {
            NPCModel = npc;
            NPCSpawnModel = npcSpawn;
            m_stats = stats;
            m_fiber = fiber;

            Position = new Vector2((float)npcSpawn.X, (float)npcSpawn.Y);
            ID = IDGenerator.GetNextID();

            Health = Formulas.StaminaToHealth(m_stats[StatType.Stamina]);
            MaxHealth = Health;

            StateUpdate = new NPCStateUpdate()
            {
                Rotation = npcSpawn.Rotation,
                NPCID = npc.NPCID,
                NPCInstanceID = ID,
                Health = Health,
                MaxHealth = MaxHealth
            };

            m_behaviours = behaviours;
        }

        public void Update(TimeSpan dt)
        {
            if (Dead)
            {
                return;
            }

            foreach (INPCBehaviour behaviour in m_behaviours)
            {
                behaviour.Update(dt, this);
            }

            StateUpdate.X = Position.X;
            StateUpdate.Y = Position.Y;
            StateUpdate.Rot = Rotation;
            StateUpdate.VelX = Velocity.X;
            StateUpdate.VelY = Velocity.Y;
            StateUpdate.Health = Health;
            StateUpdate.MaxHealth = MaxHealth;
        }

        public void ApplyHealthDelta(int delta)
        {
            int newHealth = Health + delta;

            Health = MathHelper.Clamp(newHealth, 0, MaxHealth);

            if (Health == 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Dead = true;
            m_fiber.Schedule(Respawn, NPCSpawnModel.Frequency);

            Info("Died");
        }

        private void Respawn()
        {
            Position = new Vector2((float)NPCSpawnModel.X, (float)NPCSpawnModel.Y);
            Rotation = NPCSpawnModel.Rotation;

            ID = IDGenerator.GetNextID();

            Health = MaxHealth;
            Dead = false;

            Info("Respawned");
        }

        public UseAbilityResult AcceptAbilityAsSource(AbilityInstance ability)
        {
            ApplyHealthDelta(ability.Ability.SourceHealthDelta);
            return UseAbilityResult.Completed;
        }

        public Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance ability)
        {
            return m_fiber.Enqueue(() =>
            {
                ApplyHealthDelta(ability.Ability.TargetHealthDelta);
                return UseAbilityResult.Completed;
            });
        }

        #region Logging
        private const string LOG_FORMAT = "[{0}] {1}: {2}";
        private void Trace(string message, params object[] args)
        {
            s_log.Trace(string.Format(LOG_FORMAT, ID, Name, string.Format(message, args)));
        }
        private void Info(string message, params object[] args)
        {
            s_log.Info(string.Format(LOG_FORMAT, ID, Name, string.Format(message, args)));
        }
        private void Warn(string message, params object[] args)
        {
            s_log.Warn(string.Format(LOG_FORMAT, ID, Name, string.Format(message, args)));
        }
        private void Error(string message, params object[] args)
        {
            s_log.Error(string.Format(LOG_FORMAT, ID, Name, string.Format(message, args)));
        }
        #endregion
    }
}
