using Data.NPCs;
using Protocol;
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
        public float Rotation { get; private set; }

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public int Power { get; private set; }
        public int MaxPower { get; private set; }

        public int? TargetID { get; private set; }

        public int TimeOnClient { get; private set; }

        public Zone CurrentZone { get; private set; }

        private void Handle_PlayerStateUpdate(PlayerStateUpdate_C2S psu)
        {
            Rotation = psu.Rot;
            Position = new Vector2(psu.X, psu.Y);
            Velocity = new Vector2(psu.VelX, psu.VelY);
            TimeOnClient = psu.Time;
            TargetID = psu.TargetID;
        }

        private void BuildAndSendWorldStateUpdate()
        {
            if (LatestStateUpdate == null)
            {
                return;
            }

            m_worldState.PlayerStates.Clear();
            m_worldState.PlayerIntroductions.Clear();

            m_worldState.CurrentServerTime = Environment.TickCount;
            m_worldState.Health = Health;
            m_worldState.MaxHealth = MaxHealth;
            m_worldState.Power = Power;
            m_worldState.MaxPower = MaxPower;

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
    }
}
