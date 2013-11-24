using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Map
{
    public class WaypointConnection
    {
        public Waypoint From { get; private set; }
        public Waypoint To { get; private set; }
        public float Cost { get; private set; }

        public WaypointConnection(Waypoint from, Waypoint to)
        {
            From = from;
            To = to;

            Cost = Vector2.DistanceSquared(from.Position, to.Position);
        }
    }
}
