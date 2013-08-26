using Data.Abilities;
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

        public Vector2 Velocity { get; set; }

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public int Power { get; private set; }
        public int MaxPower { get; private set; }

        public byte Rotation { get; set; }

        public bool IsDead { get; private set; }

        public byte Level { get { return 1; } } 

        public NPCInstance(Fiber fiber, NPCModel npc, NPCSpawnModel npcSpawn, List<INPCBehaviour> behaviours, IReadOnlyDictionary<StatType, float> stats)
        {
            NPCModel = npc;
            NPCSpawnModel = npcSpawn;
            m_stats = stats;
            m_fiber = fiber;

            Position = new Vector2((float)npcSpawn.X, (float)npcSpawn.Y);
            ID = IDGenerator.GetNextID();

            Health = Formulas.StaminaToHealth(GetStatValue(StatType.Stamina));
            MaxHealth = Health;

            StateUpdate = new NPCStateUpdate()
            {
                Rot = Compression.RotationToByte(npcSpawn.Rotation),
                NPCID = npc.NPCID,
                NPCInstanceID = ID,
                Health = Health,
                Power = 100,
            };

            m_behaviours = behaviours;
        }

        public void Update(TimeSpan dt)
        {
            if (IsDead)
            {
                return;
            }

            foreach (INPCBehaviour behaviour in m_behaviours)
            {
                behaviour.Update(dt, this);
            }

            StateUpdate.X = Compression.PositionToUShort(Position.X);
            StateUpdate.Y = Compression.PositionToUShort(Position.Y);
            StateUpdate.Rot = Compression.RotationToByte(Rotation);
            StateUpdate.VelX = Compression.VelocityToShort(Velocity.X);
            StateUpdate.VelY = Compression.VelocityToShort(Velocity.Y);
            StateUpdate.Health = (ushort)Health;
            StateUpdate.Time = Environment.TickCount;
        }

        public void ApplyHealthDelta(int delta, ITargetable source)
        {
            int newHealth = Health + delta;

            Health = MathHelper.Clamp(newHealth, 0, MaxHealth);

            if (Health == 0)
            {
                Die(source);
            }
        }

        public void ApplyPowerDelta(int delta, ITargetable source)
        {
            int newPower = Power + delta;

            Power = MathHelper.Clamp(newPower, 0, MaxPower);
        }

        public void ApplyXPDelta(int delta, ITargetable source)
        {
        }

        private void Die(ITargetable killer)
        {
            IsDead = true;
            m_fiber.Schedule(Respawn, NPCSpawnModel.Frequency);

            Info("Killed by {0}", killer == null ? "[Unknown]" : killer.Name);
        }

        private void Respawn()
        {
            Position = new Vector2((float)NPCSpawnModel.X, (float)NPCSpawnModel.Y);
            Rotation = Compression.RotationToByte(NPCSpawnModel.Rotation);

            ID = IDGenerator.GetNextID();

            Health = MaxHealth;
            IsDead = false;

            Info("Respawned");
        }

        public UseAbilityResult AcceptAbilityAsSource(AbilityInstance ability)
        {
            ApplyHealthDelta(ability.Ability.SourceHealthDelta, this);
            return UseAbilityResult.OK;
        }

        public Task<UseAbilityResult> AcceptAbilityAsTarget(AbilityInstance ability)
        {
            return m_fiber.Enqueue(() =>
            {
                if (IsDead)
                {
                    return UseAbilityResult.InvalidTarget;
                }
                else if (Vector2.DistanceSquared(ability.Source.Position, Position) > Math.Pow(ability.Ability.Range, 2))
                {
                    return UseAbilityResult.OutOfRange;
                }
                else
                {
                    int levelBonus = ability.Source.Level * 5;
                    if (ability.Ability.AbilityType == AbilityModel.EAbilityType.HARM)
                    {
                        levelBonus *= -1;
                    }
                    ApplyHealthDelta(ability.Ability.TargetHealthDelta + levelBonus, ability.Source);
                    return UseAbilityResult.OK;
                }
            });
        }

        public void AwardXP(float xp)
        {
        }

        private float GetStatValue(StatType statType)
        {
            float stat;

            m_stats.TryGetValue(statType, out stat);

            return stat;
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
