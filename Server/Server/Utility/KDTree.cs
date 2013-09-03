using Server.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Utility
{
    public class KDTree<T> where T : IPositionable
    {
        private class Node
        {
            public int Left { get; set; }
            public int Right { get; set; }
            public BoundingBox Bounds { get; set; }

            public Node()
            {
                Left = -1;
                Right = -1;
            }
        }

        private T[] m_entities = new T[0];
        private List<Node> m_nodes = new List<Node>();
        private ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        private int m_root;

        public void Build(T[] entities)
        {
            if (entities.Length > 0)
            {
                m_lock.EnterWriteLock();
                m_entities = entities.ToArray();
                int count = m_entities.Length;
                if (m_nodes.Capacity < count)
                {
                    m_nodes.Capacity = count;
                }
                while (m_nodes.Count < count)
                {
                    m_nodes.Add(new Node());
                }

                m_root = KDSort(m_entities, m_nodes, 0, count - 1, false);
                m_lock.ExitWriteLock();
            }
        }

        public List<T> GatherRange(BoundingBox range)
        {
            List<T> result = new List<T>();

            if (m_entities.Length > 0)
            {
                m_lock.EnterReadLock();
                Query(m_entities, m_nodes, ref range, m_root, result);
                m_lock.ExitReadLock();
            }

            return result;
        }

        private static int KDSort(T[] values, List<Node> nodes, int left, int right, bool sortByX)
        {
            int pivot = Partition(values, (left + right) / 2, left, right, sortByX);

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

            nodes[pivot].Bounds = bounds;

            Node link = nodes[pivot];

            link.Left = -1;
            link.Right = -1;
            if (left < pivot)
            {
                link.Left = KDSort(values, nodes, left, pivot - 1, !sortByX);
            }

            if (right > pivot)
            {
                link.Right = KDSort(values, nodes, pivot + 1, right, !sortByX);
            }

            return pivot;
        }

        private static int Partition(T[] values, int pivot, int left, int right, bool sortByX)
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

        private static void Query(T[] values, List<Node> nodes, ref BoundingBox range, int root, IList<T> result)
        {
            ContainmentType ct = range.Contains(nodes[root].Bounds);

            if (ct == ContainmentType.Contains)
            {
                GatherAll(values, nodes, root, result);
            }
            else if (ct == ContainmentType.Intersects)
            {
                Node link = nodes[root];
                if (link.Left >= 0)
                {
                    Query(values, nodes, ref range, link.Left, result);
                }

                if (link.Right >= 0)
                {
                    Query(values, nodes, ref range, link.Right, result);
                }

                if (range.Contains(values[root].Position) != ContainmentType.Disjoint)
                {
                    result.Add(values[root]);
                }
            }
        }

        private static void GatherAll(T[] values, List<Node> nodes, int root, IList<T> result)
        {
            result.Add(values[root]);

            Node link = nodes[root];
            if (link.Left >= 0)
            {
                GatherAll(values, nodes, link.Left, result);
            }

            if (link.Right >= 0)
            {
                GatherAll(values, nodes, link.Right, result);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(T[] values, int a, int b)
        {
            T tmp = values[a];
            values[a] = values[b];
            values[b] = tmp;
        }
    }
}
