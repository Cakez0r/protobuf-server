
namespace Server.Map
{
    public class WaypointConnection
    {
        public Waypoint Source { get; private set; }
        public Waypoint Target { get; private set; }

        public WaypointConnection(Waypoint from, Waypoint to)
        {
            Source = from;
            Target = to;
        }

        public bool Equals(WaypointConnection other)
        {
            return other == this;
        }
    }
}
