using Protocol;
using Server.Abilities;
using Server.Gameplay;
using Server.Utility;
using Server.Zones;
using System;
using System.Collections.Generic;

namespace Server
{
    public partial class PlayerPeer
    {
        private static readonly Vector2 RELEVANCE_RANGE = new Vector2(40, 40);

        private Dictionary<int, Zone> m_zones;

        private WorldState m_worldState = new WorldState();

        private Dictionary<int, EntityIntroduction> m_introducedEntities = new Dictionary<int, EntityIntroduction>();

        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public byte Rotation { get; private set; }

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public int Power { get; private set; }
        public int MaxPower { get; private set; }

        public byte Level { get; private set; }

        public bool IsDead { get { return false;  } }

        public int TargetID { get; private set; }

        public int TimeOnClient { get; private set; }

        public Zone CurrentZone { get; private set; }

        private EntityStateUpdate m_latestStateUpdate;
        private EntityIntroduction m_introduction;

        private List<IEntity> m_nearEntities = new List<IEntity>();

        private short m_compressedVelX;
        private short m_compressedVelY;
        private ushort m_compressedX;
        private ushort m_compressedY;

        private void Handle_PlayerStateUpdate(PlayerStateUpdate psu)
        {
            if (m_lastAbility.State == AbilityState.Casting && (psu.X != m_compressedX || psu.Y != m_compressedY))
            {
                StopCasting();
            }

            Rotation = psu.Rot;
            Position = new Vector2(Compression.UShortToPosition(psu.X), Compression.UShortToPosition(psu.Y));
            Velocity = new Vector2(Compression.ShortToVelocity(psu.VelX), Compression.ShortToVelocity(psu.VelY));
            TimeOnClient = psu.Time;
            TargetID = psu.TargetID;

            m_compressedVelX = psu.VelX;
            m_compressedVelY = psu.VelY;
            m_compressedX = psu.X;
            m_compressedY = psu.Y;
        }

        private void BuildAndSendWorldStateUpdate()
        {
            m_worldState.CurrentServerTime = Environment.TickCount;
            m_worldState.Health = Health;
            m_worldState.Power = Power;

            List<EntityStateUpdate> entityStates = new List<EntityStateUpdate>();

            m_worldState.EntityIntroductions = null;

            BoundingBox range = new BoundingBox(Position - RELEVANCE_RANGE, Position + RELEVANCE_RANGE);
            List<IEntity> nearEntities = CurrentZone.GatherEntitiesInRange(range);

            for (int i = 0; i < nearEntities.Count; i++) 
            {
                IEntity entity = nearEntities[i];
                EntityStateUpdate esu = entity.GetStateUpdate();
                if (entity.ID != ID && esu != null && !entity.IsDead)
                {
                    entityStates.Add(entity.GetStateUpdate());

                    EntityIntroduction introduction = entity.GetIntroduction();
                    if (!m_introducedEntities.ContainsKey(esu.ID) || m_introducedEntities[esu.ID] != introduction)
                    {
                        if (m_worldState.EntityIntroductions == null)
                        {
                            m_worldState.EntityIntroductions = new List<EntityIntroduction>();
                        }

                        m_worldState.EntityIntroductions.Add(introduction);
                        m_introducedEntities[esu.ID] = introduction;
                    }
                }
            }

            m_worldState.EntityStates = entityStates;
            m_nearEntities = nearEntities;

            Send(m_worldState);
        }

        public void ApplyHealthDelta(int delta, IEntity source)
        {
            int newHealth = Health + delta;

            Health = (ushort)MathHelper.Clamp(newHealth, 0, MaxHealth);

            if (Health == 0)
            {
                Die(source);
            }
        }

        public void ApplyPowerDelta(int delta, IEntity source)
        {
            int power = Power + delta;

            Power = (ushort)MathHelper.Clamp(power, 0, MaxPower);
        }

        public void ApplyXPDelta(int delta, IEntity source)
        {
            Fiber.Enqueue(() =>
            {
                Trace("Awarded {0}xp from {1}", delta, source.Name);

                float newXP = GetStatValue(StatType.XP) + delta;
                m_stats[StatType.XP].StatValue = newXP;

                byte newLevel = Formulas.XPToLevel(newXP);
                if (newLevel > Level)
                {
                    Level = newLevel;
                    MaxPower = (ushort)Formulas.LevelToPower(Level);
                    RecreateIntroduction();
                    Info("Dinged level {0}", newLevel);
                }

                SendStatChanges(StatType.XP);
            });
        }

        private void Die(IEntity killer)
        {
            Health = MaxHealth;
            Power = MaxPower;
            Warp(0, (float)m_player.X, (float)m_player.Y);
            StopCasting();
            Info("Killed by {0}", killer == null ? "[Unknown]" : killer.Name);
        }

        private void ChangeZone(int newZoneID)
        {
            if (CurrentZone == null || newZoneID != CurrentZone.ID)
            {
                Zone newZone = default(Zone);
                if (m_zones.TryGetValue(newZoneID, out newZone))
                {
                    if (CurrentZone != null)
                    {
                        CurrentZone.RemoveFromZone(this);
                    }
                    newZone.AddToZone(this);
                    CurrentZone = newZone;
                }
            }
        }

        private void Warp(int zoneID, float x, float y)
        {
            if (CurrentZone == null || zoneID != CurrentZone.ID)
            {
                ChangeZone(zoneID);
            }

            Send(new Warp() { ZoneID = zoneID, X = x, Y = y });
        }

        public EntityStateUpdate GetStateUpdate()
        {
            return m_latestStateUpdate;
        }

        public EntityIntroduction GetIntroduction()
        {
            return m_introduction;
        }
    }
}
