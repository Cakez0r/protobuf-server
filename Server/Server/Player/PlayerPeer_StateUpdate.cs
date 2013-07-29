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
        public PlayerIntroduction Introduction
        {
            get;
            private set;
        }

        public PlayerStateUpdate_S2C LatestStateUpdate
        {
            get;
            private set;
        }

        private HashSet<int> m_introducedNPCs = new HashSet<int>();
        private INPCRepository m_npcRepository;

        private void Handle_PlayerStateUpdate(PlayerStateUpdate_C2S psu)
        {
            m_playerStateAccessor.Transaction((s) =>
            {
                s.Rotation = psu.Rot;
                s.Position = new Vector2(psu.X, psu.Y);
                s.Velocity = new Vector2(psu.VelX, psu.VelY);
                s.TimeOnClient = psu.Time;
                s.TargetID = psu.TargetID;
            });
        }

        private void BuildAndSendWorldStateUpdate()
        {
            m_worldState.PlayerStates.Clear();
            m_worldState.PlayerIntroductions.Clear();

            m_worldState.CurrentServerTime = Environment.TickCount;

            foreach (PlayerPeer player in m_playerState.CurrentZone.PlayersInZone)
            {
                if (player.ID != ID && player.LatestStateUpdate != null)
                {
                    PlayerStateUpdate_S2C stateUpdate = player.LatestStateUpdate;
                    float distanceSqr = Vector2.DistanceSquared(m_playerState.Position, new Vector2(stateUpdate.X, stateUpdate.Y));
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

            m_playerState.CurrentZone.GatherNPCStatesForPlayer(this, m_worldState.NPCStates);

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
            m_playerStateAccessor.Transaction((s) =>
            {
                if (s.CurrentZone == null || newZoneID != s.CurrentZone.ID)
                {
                    Zone newZone = default(Zone);
                    if (m_zones.TryGetValue(newZoneID, out newZone))
                    {
                        if (s.CurrentZone != null)
                        {
                            s.CurrentZone.RemoveFromZone(this);
                        }
                        newZone.AddToZone(this);
                        s.CurrentZone = newZone;
                    }
                }
            });
        }
    }
}
