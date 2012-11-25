using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Zones
{
    public class ZoneManager
    {
        private Dictionary<int, Zone> m_zones = new Dictionary<int, Zone>();

        public ZoneManager()
        {
            for (int i = 0; i < 5; i++)
            {
                m_zones.Add(i, new Zone(i));
            }
        }

        public void Update(TimeSpan dt)
        {
            Parallel.ForEach(m_zones.Values, zone =>
            {
                zone.Update(dt);
            });
        }

        public bool EnterZone(PlayerContext p, int zoneID)
        {
            Zone zone = null;

            if (m_zones.TryGetValue(zoneID, out zone))
            {
                zone.AddPlayer(p);
                return true;
            }

            return false;
        }

        public bool TransferZone(PlayerContext p, int oldZoneID, int newZoneID)
        {
            Zone oldZone = null;
            Zone newZone = null;
            if (m_zones.TryGetValue(oldZoneID, out oldZone))
            {
                if (m_zones.TryGetValue(newZoneID, out newZone))
                {
                    if (oldZone.IsPlayerInZone(p))
                    {
                        oldZone.RemovePlayer(p);
                        newZone.AddPlayer(p);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
