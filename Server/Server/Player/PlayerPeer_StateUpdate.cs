using Data.NPCs;
using Protocol;
using Server.Abilities;
using Server.Gameplay;
using Server.NPC;
using Server.Utility;
using Server.Zones;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Server
{
    public partial class PlayerPeer
    {
        private static readonly Vector2 RELEVANCE_RANGE = new Vector2(40, 40);

        private Dictionary<int, Zone> m_zones;

        private WorldState m_worldState = new WorldState();

        private Dictionary<int, PlayerIntroduction> m_introducedPlayers = new Dictionary<int, PlayerIntroduction>();
        public PlayerIntroduction Introduction { get; private set; }

        private HashSet<int> m_introducedNPCs = new HashSet<int>();
        private INPCRepository m_npcRepository;

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

        private PlayerStateUpdate_S2C m_latestStateUpdate;

        private short m_compressedVelX;
        private short m_compressedVelY;
        private ushort m_compressedX;
        private ushort m_compressedY;

        private void Handle_PlayerStateUpdate(PlayerStateUpdate_C2S psu)
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
            m_worldState.MaxHealth = MaxHealth;
            m_worldState.Power = Power;
            m_worldState.MaxPower = MaxPower;
            m_worldState.XP = GetStatValue(StatType.XP);

            m_worldState.PlayerIntroductions = null;
            m_worldState.NPCIntroductions = null;

            List<EntityStateUpdate> entityStates = new List<EntityStateUpdate>();

            BoundingBox range = new BoundingBox(Position - RELEVANCE_RANGE, Position + RELEVANCE_RANGE);
            List<PlayerPeer> nearPlayers = CurrentZone.GatherPlayersInRange(range);

            for (int i = 0; i < nearPlayers.Count; i++) 
            {
                PlayerPeer player = nearPlayers[i];
                EntityStateUpdate esu = player.GetStateUpdate();
                if (player.ID != ID && esu != null)
                {
                    entityStates.Add(player.GetStateUpdate());

                    PlayerIntroduction introduction = player.Introduction;
                    if (!m_introducedPlayers.ContainsKey(esu.ID) || m_introducedPlayers[esu.ID] != introduction)
                    {
                        if (m_worldState.PlayerIntroductions == null)
                        {
                            m_worldState.PlayerIntroductions = new List<PlayerIntroduction>();
                        }

                        m_worldState.PlayerIntroductions.Add(introduction);
                        m_introducedPlayers[esu.ID] = introduction;
                    }
                }
            }

            List<NPCInstance> nearNPCs = CurrentZone.GatherNPCSInRange(range);

            for (int i = 0; i < nearNPCs.Count; i++)
            {
                NPCInstance npc = nearNPCs[i];
                if (!npc.IsDead)
                {
                    EntityStateUpdate esu = npc.GetStateUpdate();
                    entityStates.Add(esu);

                    if (!m_introducedNPCs.Contains(esu.ID))
                    {
                        if (m_worldState.NPCIntroductions == null)
                        {
                            m_worldState.NPCIntroductions = new List<NPCIntroduction>();
                        }
                        m_worldState.NPCIntroductions.Add(npc.Introduction);
                        m_introducedNPCs.Add(npc.NPCModel.NPCID);
                    }
                }
            }

            m_worldState.EntityStates = entityStates;


            foreach (NPCStateUpdate nsu in m_worldState.EntityStates.Where(s => s is NPCStateUpdate))
            {
                if (!m_introducedNPCs.Contains(nsu.ID))
                {
                    if (m_worldState.NPCIntroductions == null)
                    {
                        m_worldState.NPCIntroductions = new List<NPCIntroduction>();
                    }

                    NPCModel npcModel = m_npcRepository.GetNPCByID(nsu.ID);
                    if (npcModel != null)
                    {
                        NPCIntroduction introduction = new NPCIntroduction()
                        {
                            Level = 1,
                            MaxHealth = 200,
                            MaxPower = 200,
                            Model = npcModel.Model,
                            Name = npcModel.Name,
                            NPCID = npcModel.NPCID,
                            Scale = npcModel.Scale
                        };

                        m_worldState.NPCIntroductions.Add(introduction);
                        m_introducedNPCs.Add(nsu.ID);
                    }
                }
            }

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
                    Info("Dinged level {0}", newLevel);
                }
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
    }
}
