using Data.NPCs;
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
        private const int RELEVANCE_DISTANCE_SQR = 40 * 40;

        private Dictionary<int, Zone> m_zones;

        private WorldState m_worldState = new WorldState() 
        { 
            PlayerStates = new List<PlayerStateUpdate_S2C>(),
            PlayerIntroductions = new List<PlayerIntroduction>(),
            NPCStates = new List<NPCStateUpdate>(),
            NPCIntroductions = new List<NPCIntroduction>()
        };

        private HashSet<int> m_introducedPlayers = new HashSet<int>();
        public PlayerIntroduction Introduction { get; private set; }

        public PlayerStateUpdate_S2C LatestStateUpdate { get; private set; }

        private HashSet<int> m_introducedNPCs = new HashSet<int>();
        private INPCRepository m_npcRepository;

        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public byte Rotation { get; private set; }

        public ushort Health { get; private set; }
        public ushort MaxHealth { get; private set; }

        public ushort Power { get; private set; }
        public ushort MaxPower { get; private set; }

        public byte Level { get; private set; }

        public int? TargetID { get; private set; }

        public int TimeOnClient { get; private set; }

        public Zone CurrentZone { get; private set; }

        private PlayerStateUpdate_C2S m_lastPlayerStateReceived = new PlayerStateUpdate_C2S();

        private void Handle_PlayerStateUpdate(PlayerStateUpdate_C2S psu)
        {
            if (m_spellCastCancellationToken != null && (psu.X != Position.X || psu.Y != Position.Y))
            {
                StopCasting();
            }

            Rotation = psu.Rot;
            Position = new Vector2(Compression.UShortToPosition(psu.X), Compression.UShortToPosition(psu.Y));
            Velocity = new Vector2(Compression.ShortToVelocity(psu.VelX), Compression.ShortToVelocity(psu.VelY));
            TimeOnClient = psu.Time;
            TargetID = psu.TargetID;

            m_lastPlayerStateReceived = psu;
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

            foreach (PlayerPeer player in CurrentZone.PlayersInZone)
            {
                if (player.ID != ID && player.LatestStateUpdate != null)
                {
                    PlayerStateUpdate_S2C stateUpdate = player.LatestStateUpdate;
                    float distanceSqr = Vector2.DistanceSquared(Position, new Vector2(stateUpdate.X, stateUpdate.Y));
                    if (distanceSqr <= RELEVANCE_DISTANCE_SQR)
                    {
                        m_worldState.PlayerStates.Add(stateUpdate);
                        if (!m_introducedPlayers.Contains(stateUpdate.PlayerID))
                        {
                            m_worldState.PlayerIntroductions.Add(player.Introduction);
                            m_introducedPlayers.Add(stateUpdate.PlayerID);
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
                        Scale = npc.Scale
                    };
                    m_worldState.NPCIntroductions.Add(introduction);
                    m_introducedNPCs.Add(nsu.NPCID);
                }
            }

            Send(m_worldState);
        }

        private void ApplyHealthDelta(int delta, ITargetable source = null)
        {
            int newHealth = Health + delta;

            Health = (ushort)MathHelper.Clamp(newHealth, 0, MaxHealth);

            if (Health == 0)
            {
                Die(source);
            }
        }

        private void ApplyPowerDelta(int delta, ITargetable source = null)
        {
            int power = Power + delta;

            Power = (ushort)MathHelper.Clamp(power, 0, MaxPower);
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
