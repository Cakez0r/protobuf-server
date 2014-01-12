using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Utility
{
    //Taken from MonoGame 27/08/2013

    public enum ContainmentType
    {
        Disjoint,
        Contains,
        Intersects
    }

    public struct BoundingBox : IEquatable<BoundingBox>
    {
        #region Public Fields

        public Vector2 Min;

        public Vector2 Max;

        public const int CornerCount = 8;

        #endregion Public Fields


        #region Public Constructors

        public BoundingBox(Vector2 min, Vector2 max)
        {
            this.Min = min;
            this.Max = max;
        }

        #endregion Public Constructors


        #region Public Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ContainmentType Contains(BoundingBox box)
        {
            //test if all corner is in the same side of a face by just checking min and max
            if (box.Max.X < Min.X
                || box.Min.X > Max.X
                || box.Max.Y < Min.Y
                || box.Min.Y > Max.Y)
                return ContainmentType.Disjoint;


            if (box.Min.X >= Min.X
                && box.Max.X <= Max.X
                && box.Min.Y >= Min.Y
                && box.Max.Y <= Max.Y)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        public void Contains(ref BoundingBox box, out ContainmentType result)
        {
            result = Contains(box);
        }

        public ContainmentType Contains(Vector2 point)
        {
            ContainmentType result;
            this.Contains(ref point, out result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(Vector2 point)
        {
            this.Min.X = Math.Min(point.X, this.Min.X);
            this.Min.Y = Math.Min(point.Y, this.Min.Y);
            this.Max.X = Math.Max(point.X, this.Max.X);
            this.Max.Y = Math.Max(point.Y, this.Max.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(BoundingBox bounds)
        {
            Encapsulate(bounds.Min);
            Encapsulate(bounds.Max);
        }

        public void Contains(ref Vector2 point, out ContainmentType result)
        {
            //first we get if point is out of box
            if (point.X < this.Min.X
                || point.X > this.Max.X
                || point.Y < this.Min.Y
                || point.Y > this.Max.Y)
            {
                result = ContainmentType.Disjoint;
            }//or if point is on box because coordonate of point is lesser or equal
            else if (point.X == this.Min.X
                || point.X == this.Max.X
                || point.Y == this.Min.Y
                || point.Y == this.Max.Y)
                result = ContainmentType.Intersects;
            else
                result = ContainmentType.Contains;


        }

        public static BoundingBox CreateFromPoints(IEnumerable<Vector2> points)
        {
            if (points == null)
                throw new ArgumentNullException();

            // TODO: Just check that Count > 0
            bool empty = true;
            Vector2 vector2 = new Vector2(float.MaxValue);
            Vector2 vector1 = new Vector2(float.MinValue);
            foreach (Vector2 vector3 in points)
            {
                vector2 = Vector2.Min(vector2, vector3);
                vector1 = Vector2.Max(vector1, vector3);
                empty = false;
            }
            if (empty)
                return new BoundingBox(Vector2.Zero, Vector2.Zero);

            return new BoundingBox(vector2, vector1);
        }

        public static BoundingBox CreateMerged(BoundingBox original, BoundingBox additional)
        {
            return new BoundingBox(
                Vector2.Min(original.Min, additional.Min), Vector2.Max(original.Max, additional.Max));
        }

        public static void CreateMerged(ref BoundingBox original, ref BoundingBox additional, out BoundingBox result)
        {
            result = BoundingBox.CreateMerged(original, additional);
        }

        public bool Equals(BoundingBox other)
        {
            return (this.Min == other.Min) && (this.Max == other.Max);
        }

        public override bool Equals(object obj)
        {
            return (obj is BoundingBox) ? this.Equals((BoundingBox)obj) : false;
        }

        public Vector2[] GetCorners()
        {
            return new Vector2[] {
                new Vector2(this.Min.X, this.Max.Y), 
                new Vector2(this.Max.X, this.Max.Y),
                new Vector2(this.Max.X, this.Min.Y), 
                new Vector2(this.Min.X, this.Min.Y), 
            };
        }

        public void GetCorners(Vector2[] corners)
        {
            if (corners == null)
            {
                throw new ArgumentNullException("corners");
            }
            if (corners.Length < 8)
            {
                throw new ArgumentOutOfRangeException("corners", "Not Enought Corners");
            }
            corners[0].X = this.Min.X;
            corners[0].Y = this.Max.Y;
            corners[1].X = this.Max.X;
            corners[1].Y = this.Max.Y;
            corners[2].X = this.Max.X;
            corners[2].Y = this.Min.Y;
            corners[3].X = this.Min.X;
            corners[3].Y = this.Min.Y;
        }

        public override int GetHashCode()
        {
            return this.Min.GetHashCode() + this.Max.GetHashCode();
        }

        public bool Intersects(BoundingBox box)
        {
            bool result;
            Intersects(ref box, out result);
            return result;
        }

        public void Intersects(ref BoundingBox box, out bool result)
        {
            if ((this.Max.X >= box.Min.X) && (this.Min.X <= box.Max.X))
            {
                result = (this.Max.Y < box.Min.Y) || (this.Min.Y > box.Max.Y);
                return;
            }

            result = false;
            return;
        }

        public static bool operator ==(BoundingBox a, BoundingBox b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BoundingBox a, BoundingBox b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
            return string.Format("{{Min:{0} Max:{1}}}", this.Min.ToString(), this.Max.ToString());
        }

        #endregion Public Methods
    }
}