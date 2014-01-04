using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Utility
{
    /// <summary>
    /// An (imbalanced) two-dimensional KDTree that can be quickly queried for points within a given range.
    /// </summary>
    public class PointKDTree<T> where T : IPositionable
    {
        /// <summary>
        /// Internal structure to represent nodes in the tree
        /// </summary>
        private class Node
        {
            /// <summary>
            /// Index of the node containing objects that are positioned before this node's partition
            /// </summary>
            public int Before { get; set; }

            /// <summary>
            /// Index of the node containing objects that are positioned after this node's partition
            /// </summary>
            public int After { get; set; }

            /// <summary>
            /// The bounds of this branch of the tree and all child nodes
            /// </summary>
            public BoundingBox Bounds { get; set; }

            public Node()
            {
                Before = -1;
                After = -1;
            }
        }

        private T[] m_entities = new T[0];
        private List<Node> m_nodes = new List<Node>();
        private ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        private int m_root;

        /// <summary>
        /// Build the KDTree from the given array. 
        /// Note that the given array will be reordered and used internally. You should not mutate the array once it's been passed here.
        /// </summary>
        public void Build(T[] entities)
        {
            if (entities.Length > 0)
            {
                m_lock.EnterWriteLock();
                m_entities = entities;
                int count = m_entities.Length;
                if (m_nodes.Capacity < count)
                {
                    m_nodes.Capacity = count;
                }

                //Add more nodes to the node list if we don't have enough
                while (m_nodes.Count < count)
                {
                    m_nodes.Add(new Node());
                }

                //Start sorting!
                m_root = KDSort(m_entities, m_nodes, 0, count - 1, false);
                m_lock.ExitWriteLock();
            }
        }

        public T NearestNeighbour(Vector2 position)
        {
            int nearestIndex = 0;

            m_lock.EnterReadLock();
            nearestIndex = NearestNeighbour(m_root, m_root, float.MaxValue, position);
            m_lock.ExitReadLock();

            return m_entities[nearestIndex];
        }

        private int NearestNeighbour(int root, int nearest, float nearestDistance, Vector2 position)
        {
            Node node = m_nodes[root];

            if (node.Bounds.Contains(position) != ContainmentType.Disjoint)
            {
                float distance = Vector2.DistanceSquared(m_entities[root].Position, position);
                if (distance < nearestDistance)
                {
                    nearest = root;
                    nearestDistance = distance;
                }

                nearest = NearestNeighbour(node.Before, nearest, nearestDistance, position);
                nearest = NearestNeighbour(node.After, nearest, nearestDistance, position);
            }

            return nearest;
        }

        /// <summary>
        /// Gather all objects that are contained within the given range.
        /// </summary>
        public List<T> GatherRange(BoundingBox range)
        {
            List<T> result = new List<T>();

            GatherRange(range, result);

            return result;
        }

        /// <summary>
        /// Gather all objects that are contained within the given range.
        /// </summary>
        public void GatherRange(BoundingBox range, List<T> result)
        {
            if (m_entities.Length > 0)
            {
                m_lock.EnterReadLock();
                Query(m_entities, m_nodes, ref range, m_root, result);
                m_lock.ExitReadLock();
            }
        }

        private static int KDSort(T[] values, List<Node> nodes, int start, int end, bool sortByX)
        {
            //Partition the array by the middle element. Use the median value as the pivot to make a balanced tree (google QuickSelect).
            int pivot = Partition(values, (start + end) / 2, start, end, sortByX);

            //Create the bounds for this node
            BoundingBox bounds;
            bounds.Min = new Vector2(float.MaxValue);
            bounds.Max = new Vector2(float.MinValue);

            for (int i = start; i <= end; i++)
            {
                //Grow the bounding box if this point doesn't fit within it
                Vector2 position = values[i].Position;
                if (position.X > bounds.Max.X) bounds.Max.X = position.X;
                if (position.Y > bounds.Max.Y) bounds.Max.Y = position.Y;
                if (position.X < bounds.Min.X) bounds.Min.X = position.X;
                if (position.Y < bounds.Min.Y) bounds.Min.Y = position.Y;
            }

            nodes[pivot].Bounds = bounds;

            //Grab the current node
            Node node = nodes[pivot];
            node.Before = -1;
            node.After = -1;

            if (start < pivot)
            {
                //If there's more than one element before the pivot in this partition, divide it further
                node.Before = KDSort(values, nodes, start, pivot - 1, !sortByX);
            }

            if (end > pivot)
            {
                //If there's more than one element after the pivot in this partition, divide it further
                node.After = KDSort(values, nodes, pivot + 1, end, !sortByX);
            }

            return pivot;
        }

        private static int Partition(T[] values, int pivot, int start, int end, bool sortByX)
        {
            //Grab the position of the pivot element
            Vector2 pivotPosition = values[pivot].Position;

            //Move the pivot to the end
            Swap(values, pivot, end);

            //Keep a pointer to the next element that is less than the pivot
            int next = start;

            //Choose the pivot comparand based on whether we're partitioning on the X or Y axis
            float pivotValue = sortByX ? pivotPosition.X : pivotPosition.Y;
            for (int i = start; i < end; ++i)
            {
                //Grab the i-th  position and get the comparand
                Vector2 currentPosition = values[i].Position;
                float currentValue = sortByX ? currentPosition.X : currentPosition.Y;

                if (currentValue < pivotValue)
                {
                    //If this value is less than the pivot value, swap them in the array
                    Swap(values, i, next);

                    //Increment the pointer to the next free slot that's less than the pivot
                    next++;
                }
            }

            //Move the pivot into the next free slot that's less than the pivot.
            //Now all values before  the pivot are less than pivot and all values right of the pivot are greater
            Swap(values, end, next);

            //Return the pivot's final position as the root of this node
            return next;
        }

        private static void Query(T[] values, List<Node> nodes, ref BoundingBox range, int root, IList<T> result)
        {
            ContainmentType ct = range.Contains(nodes[root].Bounds);

            //If the given range fully contains this node, grab all child objects
            if (ct == ContainmentType.Contains)
            {
                GatherAll(values, nodes, root, result);
            }
            else if (ct == ContainmentType.Intersects)
            {
                //If it intersects, we need to check child nodes...
                Node node = nodes[root];
                if (node.Before >= 0)
                {
                    Query(values, nodes, ref range, node.Before, result);
                }

                if (node.After >= 0)
                {
                    Query(values, nodes, ref range, node.After, result);
                }

                //And check whether this node's object lies within the bounds
                if (range.Contains(values[root].Position) != ContainmentType.Disjoint)
                {
                    result.Add(values[root]);
                }
            }
        }

        private static void GatherAll(T[] values, List<Node> nodes, int root, IList<T> result)
        {
            result.Add(values[root]);

            Node node = nodes[root];
            if (node.Before >= 0)
            {
                GatherAll(values, nodes, node.Before, result);
            }

            if (node.After >= 0)
            {
                GatherAll(values, nodes, node.After, result);
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
