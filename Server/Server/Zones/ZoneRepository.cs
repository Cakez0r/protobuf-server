using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Zones
{
    public class ZoneRepository
    {
        public Dictionary<int, Zone> m_zone = new Dictionary<int, Zone>();

        public ZoneRepository()
        {
            m_zone.Add(0, new Zone(0));
            m_zone.Add(1, new Zone(1));
        }

        public Zone GetZoneByID(int zoneID)
        {
            Zone zone = default(Zone);

            m_zone.TryGetValue(zoneID, out zone);

            return zone;
        }
    }
}
