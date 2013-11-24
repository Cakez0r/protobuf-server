using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Utility
{
    public class PolygonKDTree
    {
        private class Node
        {
            public int Before { get; set; }

            public int After { get; set; }

            public BoundingBox Bounds { get; set; }

            public Node()
            {
                Before = -1;
                After = -1;
            }
        }

        private class XComparer : IComparer<Polygon>
        {
            private static IComparer<Polygon> s_instance = new XComparer();
            public static IComparer<Polygon> Instance
            {
                get { return s_instance; }
            }

            public int Compare(Polygon x, Polygon y)
            {
                return x.Center.X.CompareTo(y.Center.X);
            }
        }

        private class YComparer : IComparer<Polygon>
        {
            private static IComparer<Polygon> s_instance = new YComparer();
            public static IComparer<Polygon> Instance
            {
                get { return s_instance; }
            }

            public int Compare(Polygon x, Polygon y)
            {
                return x.Center.Y.CompareTo(y.Center.Y);
            }
        }

        private Polygon[] m_polygons = new Polygon[0];
        private List<Node> m_nodes = new List<Node>();
        private int m_root;

        public PolygonKDTree(Polygon[] polygons)
        {
            if (polygons.Length > 0)
            {
                m_polygons = polygons;
                int count = m_polygons.Length;
                if (m_nodes.Capacity < count)
                {
                    m_nodes.Capacity = count;
                }

                while (m_nodes.Count < count)
                {
                    m_nodes.Add(new Node());
                }

                m_root = KDSort(m_polygons, m_nodes, 0, count - 1, false);
            }
        }

        public List<Polygon> GatherRange(BoundingBox range)
        {
            List<Polygon> result = new List<Polygon>();

            GatherRange(range, result);

            return result;
        }

        public void GatherRange(BoundingBox range, List<Polygon> result)
        {
            if (m_polygons.Length > 0)
            {
                Query(m_polygons, m_nodes, ref range, m_root, result);
            }
        }

        private static int KDSort(Polygon[] values, List<Node> nodes, int start, int end, bool sortByX)
        {
            Array.Sort<Polygon>(values, start, end - start, sortByX ? XComparer.Instance : YComparer.Instance);
            int pivot = Partition(values, (start + end) / 2, start, end, sortByX);

            BoundingBox bounds;
            bounds.Min = new Vector2(float.MaxValue);
            bounds.Max = new Vector2(float.MinValue);

            for (int i = start; i <= end; i++)
            {
                BoundingBox bb = values[i].Bounds;
                bounds.Encapsulate(bb);
            }

            nodes[pivot].Bounds = bounds;

            Node node = nodes[pivot];
            node.Before = -1;
            node.After = -1;

            if (start < pivot)
            {
                node.Before = KDSort(values, nodes, start, pivot - 1, !sortByX);
            }

            if (end > pivot)
            {
                node.After = KDSort(values, nodes, pivot + 1, end, !sortByX);
            }

            return pivot;
        }

        private static int Partition(Polygon[] values, int pivot, int start, int end, bool sortByX)
        {
            //Grab the position of the pivot element
            Vector2 pivotPosition = values[pivot].Center;

            //Move the pivot to the end
            Swap(values, pivot, end);

            //Keep a pointer to the next element that is less than the pivot
            int next = start;

            //Choose the pivot comparand based on whether we're partitioning on the X or Y axis
            float pivotValue = sortByX ? pivotPosition.X : pivotPosition.Y;
            for (int i = start; i < end; ++i)
            {
                //Grab the i-th  position and get the comparand
                Vector2 currentPosition = values[i].Center;
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

        private static void Query(Polygon[] values, List<Node> nodes, ref BoundingBox range, int root, IList<Polygon> result)
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
                if (range.Contains(values[root].Bounds) != ContainmentType.Disjoint)
                {
                    result.Add(values[root]);
                }
            }
        }

        private static void GatherAll(Polygon[] values, List<Node> nodes, int root, IList<Polygon> result)
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
        private static void Swap(Polygon[] values, int a, int b)
        {
            Polygon tmp = values[a];
            values[a] = values[b];
            values[b] = tmp;
        }
    }
}
