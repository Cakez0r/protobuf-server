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

        public Zone GetZone(int zoneID)
        {
            Zone zone = null;

            m_zones.TryGetValue(zoneID, out zone);
            
            return zone;
        }
    }
}
