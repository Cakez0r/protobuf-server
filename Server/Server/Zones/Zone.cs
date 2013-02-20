using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.Utility;
using Protocol;
using System.Collections.Concurrent;

namespace Server.Zones
{
    public class Zone
    {
        private const float SEND_DISTANCE = 40 * 40;

        private ConcurrentDictionary<int, PlayerContext> m_playersInZone = new ConcurrentDictionary<int, PlayerContext>();

        public int ID
        {
            get;
            private set;
        }

        public Zone(int zoneID)
        {
            ID = zoneID;
        }

        public void AddPlayer(PlayerContext player)
        {
            m_playersInZone[player.ID] = player;
        }

        public bool RemovePlayer(PlayerContext player)
        {
            PlayerContext removedPlayer = default(PlayerContext);
            return m_playersInZone.TryRemove(player.ID, out removedPlayer);
        }

        public bool IsPlayerInZone(PlayerContext player)
        {
            return m_playersInZone.ContainsKey(player.ID);
        }

        public void Update(TimeSpan dt)
        {
            Parallel.ForEach(m_playersInZone, kvp1 =>
            {
                PlayerContext p1 = kvp1.Value;
                PlayerStateUpdate_S2C p1State = p1.PlayerState;
                Vector2 p1Pos = new Vector2(p1State.X, p1State.Y);
                foreach (var kvp2 in m_playersInZone)
                {
                    PlayerContext p2 = kvp2.Value;
                    PlayerStateUpdate_S2C p2State = p2.PlayerState;
                    if (p1.ID != p2.ID)
                    {
                        if (Vector2.DistanceSquared(p1Pos, new Vector2(p2State.X, p2State.Y)) < SEND_DISTANCE)
                        {
                            p1.IncludeInWorldState(p2);
                        }
                    }
                }
            });

        }

        public void SendToAllInZone(Packet o)
        {
            foreach (var kvp in m_playersInZone)
            {
                PlayerContext p = kvp.Value;
                p.Send(o);
            }
        }
    }
}
