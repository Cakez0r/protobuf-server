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
        public Waypoint Source { get; private set; }
        public Waypoint Target { get; private set; }

        public double Cost { get; private set; }

        public WaypointConnection(Waypoint from, Waypoint to)
        {
            Source = from;
            Target = to;

            Cost = Vector2.DistanceSquared(Source.Position / 1000, Target.Position / 1000);
        }

        public bool Equals(WaypointConnection other)
        {
            return other == this;
        }
    }
}
