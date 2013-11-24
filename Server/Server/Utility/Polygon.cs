using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server.Utility
{
    public class Polygon
    {
        private Vector2[] m_points;
        public Vector2[] Points 
        {
            get { return m_points; }
            set
            {
                m_points = value;
                UpdateBounds();
            }
        }

        public BoundingBox Bounds { get; private set; }

        public Vector2 Center { get; private set; }

        public Polygon()
        {

        }

        public Polygon(IEnumerable<Vector2> points)
        {
            Points = points.ToArray();
            UpdateBounds();
        }

        public bool ContainsPoint(Vector2 p)
        {
            ////http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html

            //bool result = false;

            //for (int i = 0, j = Points.Length - 1; i < Points.Length; j = i++)
            //{
            //    if 
            //    (
            //        (((Points[i].Y <= p.Y) && (p.Y < Points[j].Y)) || ((Points[j].Y <= p.Y) && (p.Y < Points[i].Y))) &&
            //        (p.X < (Points[j].X - Points[i].X) * (p.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X)
            //    )
            //    {
            //        result = !result;
            //    }
            //}

            //return result;

            int i, j;
            bool c = false;
            for (i = 0, j = Points.Length- 1; i < Points.Length; j = i++)
            {
                if ((((Points[i].Y <= p.Y) && (p.Y < Points[j].Y)) ||
                     ((Points[j].Y <= p.Y) && (p.Y < Points[i].Y))) &&
                    (p.X < (Points[j].X - Points[i].X) * (p.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X))

                    c = !c;
            }

            return c;
        }

        private void UpdateBounds()
        {
            BoundingBox bounds = new BoundingBox(new Vector2(float.MaxValue, float.MaxValue), new Vector2(float.MinValue, float.MinValue));
            foreach (Vector2 p in Points)
            {
                bounds.Encapsulate(p);
            }
            Bounds = bounds;
            Center = (bounds.Min + bounds.Max) * 0.5f;
        }
    }
}
