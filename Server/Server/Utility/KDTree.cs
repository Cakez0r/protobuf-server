using Server.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Server.Utility
{
    public class Link
    {
        public int Left;
        public int Right;
        /*
        public long a;
        public long b;
        public long c;
        public long d;
        public long e;
        public long f;
        public long g;
        public long h;
        public long i;
        public long j;
        public long k;
        public long l;
        public long m;
        public long n;
        public long o;
        */
        public Link()
        {
            Left = -1;
            Right = -1;
        }
    }

    public class KDTree
    {
        public static int LeftCount;
        public static int RightCount;

        public static int KDSort(IEntity[] values, Link[] links, BoundingBox[] boundaries, int left, int right, bool sortByX)
        {
            int pivot = Partition(values, boundaries, (left + right) / 2, left, right, sortByX);

            BoundingBox bounds;
            bounds.Min = new Vector2(float.MaxValue);
            bounds.Max = new Vector2(float.MinValue);

            for (int i = left; i <= right; i++)
            {
                Vector2 position = values[i].Position;
                if (position.X > bounds.Max.X) bounds.Max.X = position.X;
                if (position.Y > bounds.Max.Y) bounds.Max.Y = position.Y;
                if (position.X < bounds.Min.X) bounds.Min.X = position.X;
                if (position.Y < bounds.Min.Y) bounds.Min.Y = position.Y;
            }

            boundaries[pivot] = bounds;

            Link link = links[pivot];

            link.Left = -1;
            link.Right = -1;
            if (left < pivot)
            {
                link.Left = KDSort(values, links, boundaries, left, pivot - 1, !sortByX);
            }

            if (right > pivot)
            {
                link.Right = KDSort(values, links, boundaries, pivot + 1, right, !sortByX);
            }

            return pivot;
        }

        public static void Query(IEntity[] values, Link[] links, BoundingBox[] boundaries, ref BoundingBox range, int root, List<IEntity> result)
        {
            ContainmentType ct = range.Contains(boundaries[root]);

            if (ct == ContainmentType.Contains)
            {
                GatherAll(values, links, root, result);
            }
            else if (ct == ContainmentType.Intersects)
            {
                if (range.Contains(values[root].Position) != ContainmentType.Disjoint)
                {
                    result.Add(values[root]);
                }

                Link link = links[root];
                if (link.Left >= 0)
                {
                    LeftCount++;
                    Query(values, links, boundaries, ref range, link.Left, result);
                }

                if (link.Right >= 0)
                {
                    RightCount++;
                    Query(values, links, boundaries, ref range, link.Right, result);
                }
            }
        }

        private static void GatherAll(IEntity[] values, Link[] links, int root, List<IEntity> result)
        {
            result.Add(values[root]);

            Link link = links[root];
            if (link.Left >= 0)
            {
                GatherAll(values, links, link.Left, result);
            }

            if (link.Right >= 0)
            {
                GatherAll(values, links, link.Right, result);
            }
        }

        private static int Partition(IEntity[] values, BoundingBox[] boundaries, int pivot, int left, int right, bool sortByX)
        {
            Vector2 pivotPosition = values[pivot].Position;

            Swap(values, pivot, right);

            int next = left;

            float pivotValue = sortByX ? pivotPosition.X : pivotPosition.Y;
            for (int i = left; i < right; ++i)
            {
                Vector2 currentPosition = values[i].Position;
                float currentValue = sortByX ? currentPosition.X : currentPosition.Y;
                if (currentValue < pivotValue)
                {
                    Swap(values, i, next);
                    next++;
                }
            }

            Swap(values, right, next);

            return next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(IEntity[] values, int a, int b)
        {
            IEntity tmp = values[a];
            values[a] = values[b];
            values[b] = tmp;
        }
    }
}
