using Newtonsoft.Json;
using NLog;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server.Map
{
    public class MapData
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private const float MAX_PATH_LENGTH = 50000;

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
                    Waypoints[x] = new Waypoint() { Position = poly.Points[i] };
                    x++;
                }
            }

            x = 0;
            foreach (Polygon poly in CollisionAreas)
            {
                for (int i = 0; i < poly.Points.Length; i++)
                {
                    int current = x + i;
                    int next = x + ((i + 1) % poly.Points.Length);
                    Waypoints[current].AddConnectionTo(Waypoints[next]);
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

            Graphics g = Graphics.FromImage(render);
            g.Clear(Color.White);

            Pen pathPen = new Pen(Color.Green, 1);
            foreach (WaypointConnection path in Waypoints.SelectMany(w => w.Connections))
            {
                Point p1 = NormaliseVector(path.From.Position, bounds.Min, scale, size);
                Point p2 = NormaliseVector(path.To.Position, bounds.Min, scale, size);

                g.DrawLine(pathPen, p1, p2);
            }

            Pen polyPen = new Pen(Color.Red, 2);
            foreach (Polygon poly in CollisionAreas)
            {
                for (int i = 0, j = poly.Points.Length - 1; i < poly.Points.Length; j = i++)
                {
                    Point p1 = NormaliseVector(poly.Points[i], bounds.Min, scale, size);
                    Point p2 = NormaliseVector(poly.Points[j], bounds.Min, scale, size);

                    g.DrawLine(polyPen, p1, p2);
                }
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

                        Vector2 center = (from.Position + to.Position) * 0.5f;
                        if (poly.ContainsPoint(center))
                        {
                            //Path runs through a collision area
                            return false;
                        }

                        Vector2 shortenedA = Vector2.Lerp(from.Position, to.Position, 0.01f);
                        Vector2 shortenedB = Vector2.Lerp(to.Position, from.Position, 0.01f);
                        LineSegment path = new LineSegment(shortenedA, shortenedB);
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
    }
}
