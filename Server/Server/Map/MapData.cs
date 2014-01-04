using Newtonsoft.Json;
using NLog;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server.Map
{
    public class MapData
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private const float MAX_PATH_LENGTH = 50000000;

        public Polygon[] CollisionAreas { get; set; }
        public Waypoint[] Waypoints { get; set; }

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
                List<int> path = CalculateDirection(Waypoints[0], Waypoints[256]);
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

        public List<int> CalculateDirection(Waypoint from, Waypoint to)
        {
            HashSet<int> closed = new HashSet<int>();
            BinaryHeap<AStarNode> open = new BinaryHeap<AStarNode>(500);
            Dictionary<int, NodeInfo> nodeInfo = new Dictionary<int, NodeInfo>();

            AStarNode startNode = new AStarNode();
            startNode.Index = from.Index;
            startNode.H = Vector2.DistanceSquared(from.Position, to.Position);
            nodeInfo[from.Index] = new NodeInfo() { G = 0, ParentIndex = -1};
            startNode.F = startNode.H;

            open.Enqueue(startNode);

            while (open.Count > 0)
            {
                AStarNode currentNode = open.Dequeue();
                if (currentNode.Index == to.Index)
                {
                    List<int> path = new List<int>();
                    ReconstructPath(path, nodeInfo, to.Index);
                    path.Add(from.Index);
                    return path;
                }

                closed.Add(currentNode.Index);

                Waypoint currentWaypoint = Waypoints[currentNode.Index];

                int neighbourCount = currentWaypoint.Neighbours.Count;
                for (int i = 0; i < neighbourCount; i++)
                {
                    Waypoint neighbour = currentWaypoint.Neighbours[i];

                    if (closed.Contains(neighbour.Index))
                    {
                        continue;
                    }

                    float thisG = nodeInfo[currentWaypoint.Index].G + Vector2.DistanceSquared(currentWaypoint.Position, neighbour.Position);

                    int openListIndex = -1;
                    int openCount = open.Count;
                    AStarNode[] openItems = open.Items;
                    for (int j = 1; j <= openCount; j++)
                    {
                        if (openItems[j].Index == neighbour.Index)
                        {
                            openListIndex = j;
                            break;
                        }
                    }

                    bool needsAdding = openListIndex < 0;
                    if (needsAdding || (nodeInfo.ContainsKey(neighbour.Index) && thisG < nodeInfo[neighbour.Index].G))
                    {
                        nodeInfo[neighbour.Index] = new NodeInfo() { ParentIndex = currentNode.Index, G = thisG };

                        if (needsAdding)
                        {
                            AStarNode newNode = new AStarNode();
                            newNode.Index = neighbour.Index;
                            newNode.G = thisG;
                            newNode.H = Vector2.DistanceSquared(neighbour.Position, to.Position);
                            newNode.F = newNode.G + newNode.H;
                            open.Enqueue(newNode);
                        }
                        else
                        {
                            open.Items[openListIndex].G = thisG;
                            open.Items[openListIndex].F = thisG + open.Items[openListIndex].H;
                            open.Touch(openListIndex);
                        }
                    }
                }
            }

            return new List<int>();
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
