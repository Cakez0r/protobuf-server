using Server.Utility;
using System.Collections.Generic;

namespace Server.Map
{
    public class Waypoint : IPositionable
    {
        public int Index { get; private set; }

        public Vector2 Position { get; set; }

        public IEnumerable<WaypointConnection> Connections
        {
            get { return m_connections.Values; }
        }

        private Dictionary<Waypoint, WaypointConnection> m_connections = new Dictionary<Waypoint, WaypointConnection>();

        public List<Waypoint> Neighbours
        {
            get;
            private set;
        }

        public Waypoint(int index)
        {
            Index = index;
            Neighbours = new List<Waypoint>();
        }

        public WaypointConnection GetConnectionTo(Waypoint waypoint)
        {
            WaypointConnection connection = null;

            m_connections.TryGetValue(waypoint, out connection);

            return connection;
        }

        public bool IsConnectedTo(Waypoint waypoint)
        {
            return m_connections.ContainsKey(waypoint);
        }

        public void AddConnectionTo(Waypoint waypoint)
        {
            lock (m_connections)
            {
                if (!m_connections.ContainsKey(waypoint))
                {
                    m_connections.Add(waypoint, new WaypointConnection(this, waypoint));
                    Neighbours.Add(waypoint);
                }
            }

            lock (waypoint.m_connections)
            {
                if (!waypoint.m_connections.ContainsKey(this))
                {
                    waypoint.m_connections.Add(this, new WaypointConnection(waypoint, this));
                    waypoint.Neighbours.Add(this);
                }
            }
        }
    }
}
