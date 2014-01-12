using System;

namespace Server.Utility
{
    public class LineSegment
    {
        private Vector2 m_a;
        public Vector2 A
        {
            get { return m_a; }
            set
            { 
                m_a = value;
                UpdateBounds();
            }
        }

        private Vector2 m_b;
        public Vector2 B
        {
            get { return m_b; }
            set
            { 
                m_b = value;
                UpdateBounds();
            }
        }

        public BoundingBox Bounds { get; private set; }

        public LineSegment(Vector2 a, Vector2 b)
        {
            m_a = a;
            m_b = b;
            UpdateBounds();
        }

        public bool Intersects(LineSegment l)
        {
            Orientation o1 = MathHelper.CalculateOrientation(A, B, l.A);
            Orientation o2 = MathHelper.CalculateOrientation(A, B, l.B);
            Orientation o3 = MathHelper.CalculateOrientation(l.A, l.B, A);
            Orientation o4 = MathHelper.CalculateOrientation(l.A, l.B, B);

            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            if (o1 == Orientation.Colinear && Bounds.Contains(l.A) != ContainmentType.Disjoint)
            {
                return true;
            }

            if (o2 == Orientation.Colinear && Bounds.Contains(l.B) != ContainmentType.Disjoint)
            {
                return true;
            }

            if (o3 == Orientation.Colinear && l.Bounds.Contains(A) != ContainmentType.Disjoint)
            {
                return true;
            }

            if (o4 == Orientation.Colinear && l.Bounds.Contains(B) != ContainmentType.Disjoint)
            {
                return true;
            }

            return false;
        }

        private void UpdateBounds()
        {
            Bounds = new BoundingBox(new Vector2(Math.Min(m_a.X, m_b.X), Math.Min(m_a.Y, m_b.Y)), new Vector2(Math.Max(m_a.X, m_b.X), Math.Max(m_a.Y, m_b.Y)));
        }
    }
}
