using Server.Utility;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Server.Zones
{
    public class Zone
    {
        private Fiber m_fiber = new Fiber();

        private ConcurrentDictionary<int, PlayerPeer> m_playersInZone = new ConcurrentDictionary<int, PlayerPeer>();

        public IEnumerable<PlayerPeer> PlayersInZone { get; private set; }

        public int ID { get; private set; }

        public Zone(int zoneID)
        {
            PlayersInZone = Enumerable.Empty<PlayerPeer>();
            ID = zoneID;
        }
        
        public void AddToZone(PlayerPeer player)
        {
            if (m_playersInZone.TryAdd(player.ID, player))
            {
                PlayersInZone = m_playersInZone.Values;
            }
        }

        public void RemoveFromZone(PlayerPeer player)
        {
            PlayerPeer removedPlayer = default(PlayerPeer);
            if (m_playersInZone.TryRemove(player.ID, out removedPlayer))
            {
                PlayersInZone = m_playersInZone.Values;
            }
        }
    }
}
