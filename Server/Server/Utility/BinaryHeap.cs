using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Utility
{
    public class BinaryHeap<T> where T : IComparable<T>
    {
        private T[] m_items;
        private int m_itemCount;

        public int Count
        {
            get { return m_itemCount; }
        }

        public T[] Items
        {
            get { return m_items; }
        }

        private Dictionary<T, int> m_index;

        public BinaryHeap() : this(512)
        {
        }

        public BinaryHeap(int size)
        {
            m_items = new T[size];
            m_index = new Dictionary<T, int>(size);
        }

        public void Clear()
        {
            m_itemCount = 0;
            m_index.Clear();
        }

        private static void Downheap(T[] items, int i, int size, Dictionary<T, int> index)
        {
            int left = i << 1;
            int right = left + 1;
            int smallest = i;
            if (left < size && items[left].CompareTo(items[smallest]) < 0)
            {
                smallest = left;
            }

            if (right < size && items[right].CompareTo(items[smallest]) < 0)
            {
                smallest = right;
            }

            if (i != smallest)
            {
                T tmp = items[i];
                items[i] = items[smallest];
                items[smallest] = tmp;

                index[tmp] = smallest;
                index[items[i]] = i;

                Downheap(items, smallest, size, index);
            }
        }

        private static void Upheap(T[] items, int i, int size, Dictionary<T, int> index)
        {
            int parent = i >> 1;

            if (parent > 0 && items[i].CompareTo(items[parent]) < 0)
            {
                T tmp = items[i];
                items[i] = items[parent];
                items[parent] = tmp;

                index[tmp] = parent;
                index[items[i]] = i;

                Upheap(items, parent, size, index);
            }
        }

        public void Enqueue(T obj)
        {
            int index = 0;

            if (m_index.TryGetValue(obj, out index))
            {
                T old = m_items[index];
                m_items[index] = obj;
                if (obj.CompareTo(old) < 0)
                {
                    Upheap(m_items, index, m_itemCount + 1, m_index);
                }
                else
                {
                    Downheap(m_items, index, m_itemCount + 1, m_index);
                }
                m_items[index] = obj;
            }
            else
            {
                m_itemCount++;
                index = m_itemCount;
                m_items[index] = obj;
                m_index[obj] = index;
                Upheap(m_items, index, index + 1, m_index);
            }
        }

        public T Dequeue()
        {
            T ret = m_items[1];
            m_items[1] = m_items[m_itemCount];

            m_index[m_items[1]] = 1;
            //m_index.Remove(ret);

            --m_itemCount;
            Downheap(m_items, 1, m_itemCount + 1, m_index);

            return ret;
        }

        public bool Contains(T item)
        {
            return m_index.ContainsKey(item);
        }
    }
}
