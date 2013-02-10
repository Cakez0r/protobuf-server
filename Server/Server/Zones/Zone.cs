using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.Utility;
using Protocol;

namespace Server.Zones
{
    public class Zone
    {
        private Dictionary<int, PlayerContext> m_playersInZone = new Dictionary<int, PlayerContext>();
        private ReaderWriterLockSlim m_playersLock = new ReaderWriterLockSlim();

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
            m_playersLock.EnterWriteLock();
            m_playersInZone.Add(player.ID, player);
            m_playersLock.ExitWriteLock();
        }

        public void RemovePlayer(PlayerContext player)
        {
            m_playersLock.EnterWriteLock();
            m_playersInZone.Remove(player.ID);
            m_playersLock.ExitWriteLock();
        }

        public bool IsPlayerInZone(PlayerContext player)
        {
            bool ret = false;
            m_playersLock.EnterReadLock();
            ret = m_playersInZone.ContainsKey(player.ID);
            m_playersLock.ExitReadLock();
            return ret;
        }

        public void Update(TimeSpan dt)
        {
            m_playersLock.EnterReadLock();
            Parallel.ForEach(m_playersInZone.Values, p1 =>
            {
                foreach (PlayerContext p2 in m_playersInZone.Values)
                {
                    if (p1 != p2)
                    {
                        if (Vector2.Distance(new Vector2(p1.PlayerState.X, p1.PlayerState.Y), new Vector2(p2.PlayerState.X, p2.PlayerState.Y)) < 30)
                        {
                            p1.IncludeInWorldState(p2);
                        }
                    }
                }
            });

            m_playersLock.ExitReadLock();
        }

        public void SendToAllInZone(Packet o)
        {
            m_playersLock.EnterReadLock();
            foreach (PlayerContext p in m_playersInZone.Values)
            {
                p.Send(o);
            }
            m_playersLock.ExitReadLock();
        }
    }
}
