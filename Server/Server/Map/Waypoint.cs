using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Map
{
    public class Waypoint : IPositionable
    {
        public Vector2 Position { get; set; }

        public IEnumerable<WaypointConnection> Connections
        {
            get { return m_connections.Values; }
        }

        private Dictionary<Waypoint, WaypointConnection> m_connections = new Dictionary<Waypoint, WaypointConnection>();

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
            WaypointConnection connection = new WaypointConnection(this, waypoint);

            lock (m_connections)
            {
                m_connections[waypoint] = connection;
            }

            lock (waypoint.m_connections)
            {
                waypoint.m_connections[this] = connection;
            }
        }
    }
}
