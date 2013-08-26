using Data.NPCs;
using Protocol;
using Server.Abilities;
using Server.Gameplay;
using Server.Utility;
using Server.Zones;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Server
{
    public partial class PlayerPeer
    {
        private const int RELEVANCE_DISTANCE_SQR = 40 * 40;

        private Dictionary<int, Zone> m_zones;

        private WorldState m_worldState = new WorldState() 
        { 
            PlayerStates = new List<PlayerStateUpdate_S2C>(),
            PlayerIntroductions = new List<PlayerIntroduction>(),
            NPCStates = new List<NPCStateUpdate>(),
            NPCIntroductions = new List<NPCIntroduction>()
        };

        private Dictionary<int, PlayerIntroduction> m_introducedPlayers = new Dictionary<int, PlayerIntroduction>();
        public PlayerIntroduction Introduction { get; private set; }

        public PlayerStateUpdate_S2C LatestStateUpdate { get; private set; }

        private HashSet<int> m_introducedNPCs = new HashSet<int>();
        private INPCRepository m_npcRepository;

        public Vector2 m_position;
        public Vector2 Position { get { return m_position; } }
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
            m_position = new Vector2(Compression.UShortToPosition(psu.X), Compression.UShortToPosition(psu.Y));
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
            if (LatestStateUpdate == null)
            {
                return;
            }

            m_worldState.PlayerStates.Clear();
            m_worldState.PlayerIntroductions.Clear();

            m_worldState.NPCStates.Clear();
            m_worldState.NPCIntroductions.Clear();

            m_worldState.CurrentServerTime = Environment.TickCount;
            m_worldState.Health = Health;
            m_worldState.MaxHealth = MaxHealth;
            m_worldState.Power = Power;
            m_worldState.MaxPower = MaxPower;
            m_worldState.XP = GetStatValue(StatType.XP);

            ReadOnlyCollection<PlayerPeer> playersInZone = CurrentZone.PlayersInZone;
            float distanceSqr = 0.0f;
            for (int i = 0; i < playersInZone.Count; i++) 
            {
                PlayerPeer player = playersInZone[i];
                if (player.ID != ID && player.LatestStateUpdate != null)
                {
                    PlayerStateUpdate_S2C stateUpdate = player.LatestStateUpdate;
                    Vector2.DistanceSquared(ref m_position, ref player.m_position, out distanceSqr);
                    if (distanceSqr <= RELEVANCE_DISTANCE_SQR)
                    {
                        m_worldState.PlayerStates.Add(stateUpdate);
                        if (!m_introducedPlayers.ContainsKey(stateUpdate.PlayerID) || m_introducedPlayers[stateUpdate.PlayerID] != player.Introduction)
                        {
                            m_worldState.PlayerIntroductions.Add(player.Introduction);
                            m_introducedPlayers[stateUpdate.PlayerID] = player.Introduction;
                        }
                    }
                }
            }

            CurrentZone.GatherNPCStatesForPlayer(this, m_worldState.NPCStates);

            foreach (NPCStateUpdate nsu in m_worldState.NPCStates)
            {
                if (!m_introducedNPCs.Contains(nsu.NPCID))
                {
                    NPCModel npc = m_npcRepository.GetNPCByID(nsu.NPCID);
                    NPCIntroduction introduction = new NPCIntroduction()
                    {
                        Model = npc.Model,
                        Name = npc.Name,
                        NPCID = npc.NPCID,
                        Scale = npc.Scale,
                        Level = Level,
                        MaxHealth = 100,
                        MaxPower = 100
                    };
                    m_worldState.NPCIntroductions.Add(introduction);
                    m_introducedNPCs.Add(nsu.NPCID);
                }
            }

            Send(m_worldState);
        }

        public void ApplyHealthDelta(int delta, ITargetable source)
        {
            int newHealth = Health + delta;

            Health = (ushort)MathHelper.Clamp(newHealth, 0, MaxHealth);

            if (Health == 0)
            {
                Die(source);
            }
        }

        public void ApplyPowerDelta(int delta, ITargetable source)
        {
            int power = Power + delta;

            Power = (ushort)MathHelper.Clamp(power, 0, MaxPower);
        }

        public void ApplyXPDelta(int delta, ITargetable source)
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

        private void Die(ITargetable killer)
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
    }
}
