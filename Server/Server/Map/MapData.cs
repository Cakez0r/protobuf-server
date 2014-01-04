using Newtonsoft.Json;
using NLog;
using Server.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Map
{
    public class MapData
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private const float MAX_PATH_LENGTH = 50000;

        public Polygon[] CollisionAreas { get; set; }
        public Waypoint[] Waypoints { get; set; }

        private Pool<Dictionary<int, NodeInfo>> m_infoPool = new Pool<Dictionary<int, NodeInfo>>();
        private Pool<BinaryHeap<AStarNode>> m_heapPool = new Pool<BinaryHeap<AStarNode>>();
        private ConcurrentDictionary<long, List<int>> m_pathCache = new ConcurrentDictionary<long, List<int>>();

        public static MapData LoadFromFile(string path)
        {
            MapData map;
            
            using (FileStream file = File.Open(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    map = JsonConvert.DeserializeObject<MapData>(reader.ReadToEnd());
                }
                map.CalculateWaypoints();
            }

            return map;
        }

        private void CalculateWaypoints()
        {
            DateTime startTime = DateTime.Now;

            Waypoints = new Waypoint[CollisionAreas.Sum(p => p.Points.Length)];

            int x = 0;
            foreach (Polygon poly in CollisionAreas)
            {
                for (int i = 0; i < poly.Points.Length; i++)
                {
                    poly.Points[i] = poly.Points[i] * 1000;
                    Waypoints[x] = new Waypoint(x) { Position = poly.Points[i] };
                    x++;
                }
            }

            x = 0;
            foreach (Polygon poly in CollisionAreas)
            {
                for (int i = 0, j = poly.Points.Length - 1; i < poly.Points.Length; j = i++)
                {
                    Waypoints[x+i].AddConnectionTo(Waypoints[x+j]);
                }

                x += poly.Points.Length;
            }

            Parallel.ForEach(Waypoints, (w1) =>
            {
                foreach (Waypoint w2 in Waypoints)
                {
                    if (!w1.IsConnectedTo(w2))
                    {
                        if (ReachabilityTest(w1, w2))
                        {
                            w1.AddConnectionTo(w2);
                        }
                    }
                }
            });

            foreach (Waypoint waypoint in Waypoints)
            {
                waypoint.Position /= 1000;
            }

            foreach (Polygon poly in CollisionAreas)
            {
                for (int i = 0; i < poly.Points.Length; i++)
                {
                    poly.Points[i] = poly.Points[i] / 1000;
                }
            }

            TimeSpan calculationTime = DateTime.Now - startTime;

            s_log.Debug("Calculate waypoints took {0}", calculationTime);
        }

        public Bitmap RenderMap(int height)
        {
            BoundingBox bounds = BoundingBox.CreateFromPoints(CollisionAreas.SelectMany(p => p.Points));
            Vector2 scale = bounds.Max - bounds.Min;

            float aspect = scale.X / scale.Y;

            int width = (int)(height * aspect);

            Bitmap render = new Bitmap(width, height);

            Vector2 size = new Vector2(width - 24, height - 24);

            using (Graphics g = Graphics.FromImage(render))
            using (Font font = new Font("Arial", 40, FontStyle.Bold))
            using (SolidBrush b = new SolidBrush(Color.Red))
            {
                g.Clear(Color.White);

                Pen connectionPen = new Pen(Color.Green, 1);
                foreach (WaypointConnection connection in Waypoints.SelectMany(w => w.Connections))
                {
                    Point p1 = NormaliseVector(connection.Source.Position, bounds.Min, scale, size);
                    Point p2 = NormaliseVector(connection.Target.Position, bounds.Min, scale, size);

                    g.DrawLine(connectionPen, p1, p2);
                }

                Pen polyPen = new Pen(Color.Red, 3);
                foreach (Polygon poly in CollisionAreas)
                {
                    for (int i = 0, j = poly.Points.Length - 1; i < poly.Points.Length; j = i++)
                    {
                        Point p1 = NormaliseVector(poly.Points[i], bounds.Min, scale, size);
                        Point p2 = NormaliseVector(poly.Points[j], bounds.Min, scale, size);

                        g.DrawLine(polyPen, p1, p2);
                    }
                }

                Pen pathPen = new Pen(Color.Blue, 3);
                List<int> path = CalculatePath(Waypoints[0], Waypoints[256]);
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Point p1 = NormaliseVector(Waypoints[path[i]].Position, bounds.Min, scale, size);
                    Point p2 = NormaliseVector(Waypoints[path[i + 1]].Position, bounds.Min, scale, size);
                    PointF f = new PointF(p1.X, p1.Y);
                    g.DrawLine(pathPen, p1, p2);
                }

                //foreach (WaypointConnection connection in Waypoints.SelectMany(w => w.Connections))
                //{
                //    Point p1 = NormaliseVector(connection.Source.Position, bounds.Min, scale, size);

                //    g.DrawString(connection.Source.Index.ToString(), font, b, new PointF(p1.X - 20, p1.Y - 20));
                //}
            }
            return render;
        }

        private static Point NormaliseVector(Vector2 v, Vector2 offset, Vector2 scale, Vector2 size)
        {
            v -= offset;
            v /= scale;
            v *= size;

            v.Y = size.Y - v.Y;

            return new Point((int)v.X + 12, (int)v.Y + 12);
        }

        private bool ReachabilityTest(Waypoint from, Waypoint to)
        {
            if (Vector2.Distance(from.Position, to.Position) <= MAX_PATH_LENGTH)
            {
                Vector2 shortenedA = Vector2.Lerp(from.Position, to.Position, 0.0001f);
                Vector2 shortenedB = Vector2.Lerp(to.Position, from.Position, 0.0001f);

                LineSegment path = new LineSegment(shortenedA, shortenedB);

                Vector2 center = Vector2.Lerp(from.Position, to.Position, 0.5f);

                foreach (Polygon poly in CollisionAreas)
                {
                    for (int i = 0, j = poly.Points.Length - 1; i < poly.Points.Length; j = i++)
                    {
                        if ((poly.Points[i] == from.Position && poly.Points[j] == to.Position) ||
                            (poly.Points[j] == from.Position && poly.Points[i] == to.Position))
                        {
                            //Path is an edge on a collision area
                            return false;
                        }

                        if (poly.ContainsPoint(center))
                        {
                            //Path runs through a collision area
                            return false;
                        }
                        
                        LineSegment edge = new LineSegment(poly.Points[i], poly.Points[j]);
                        if (path.Intersects(edge))
                        {
                            //Path runs through a collision area
                            return false;
                        }
                    }
                }

                //Clear line of sight
                return true;
            }

            //Too far away
            return false;
        }

        struct NodeInfo
        {
            public int ParentIndex;
            public float G;
        }


        public List<int> CalculatePath(Waypoint from, Waypoint to)
        {
            List<int> path;

            long pathKey = ((long)from.Index << 32) | (uint)to.Index;
            if (m_pathCache.TryGetValue(pathKey, out path))
            {
                return path;
            }

            path = new List<int>();

            HashSet<int> closed = new HashSet<int>();
            BinaryHeap<AStarNode> open = m_heapPool.Take();
            Dictionary<int, NodeInfo> nodeInfo = m_infoPool.Take();

            int fromIndex = from.Index;
            int toIndex = to.Index;

            AStarNode startNode = new AStarNode();
            startNode.Index = fromIndex;
            startNode.H = Vector2.Distance(from.Position, to.Position);
            nodeInfo[fromIndex] = new NodeInfo() { G = 0, ParentIndex = -1 };
            startNode.F = startNode.H;

            open.Enqueue(startNode);

            while (open.Count > 0)
            {
                AStarNode currentNode = open.Dequeue();
                if (currentNode.Index == toIndex)
                {
                    ReconstructPath(path, nodeInfo, toIndex);
                    path.Add(fromIndex);

                    break;
                }

                int currentIndex = currentNode.Index;
                Waypoint currentWaypoint = Waypoints[currentIndex];

                closed.Add(currentIndex);

                int neighbourCount = currentWaypoint.Neighbours.Count;
                for (int i = 0; i < neighbourCount; i++)
                {
                    Waypoint neighbour = currentWaypoint.Neighbours[i];
                    int neighbourIndex = neighbour.Index;

                    if (closed.Contains(neighbourIndex))
                    {
                        continue;
                    }

                    float thisG = nodeInfo[currentIndex].G + Vector2.Distance(currentWaypoint.Position, neighbour.Position);

                    AStarNode newNode = new AStarNode();
                    newNode.Index = neighbourIndex;

                    if ((nodeInfo.ContainsKey(neighbourIndex) && thisG < nodeInfo[neighbourIndex].G) || !open.Contains(newNode))
                    {
                        newNode.G = thisG;
                        newNode.H = Vector2.Distance(neighbour.Position, to.Position);
                        newNode.F = newNode.G + newNode.H;
                        open.Enqueue(newNode);

                        nodeInfo[neighbourIndex] = new NodeInfo() { ParentIndex = currentIndex, G = thisG };
                    }
                }
            }

            nodeInfo.Clear();
            open.Clear();

            m_infoPool.Return(nodeInfo);
            m_heapPool.Return(open);

            m_pathCache[pathKey] = path;

            return path;
        }

        private static void ReconstructPath(List<int> path, Dictionary<int, NodeInfo> visits, int current)
        {
            if (visits.ContainsKey(current))
            {
                path.Add(current);
                ReconstructPath(path, visits, visits[current].ParentIndex);
            }
        }
    }
}
