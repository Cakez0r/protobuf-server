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
            PlayerIntroductions = new List<PlayerIntroduction>()
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
