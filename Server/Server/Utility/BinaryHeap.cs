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

        public BinaryHeap(int size)
        {
            m_items = new T[size];
        }

        private static void Downheap(T[] items, int i, int size)
        {
            int left = i * 2;
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
                Downheap(items, smallest, size);
            }
        }

        private static void Upheap(T[] items, int i, int size)
        {
            int parent = i >> 1;

            if (parent > 0 && items[i].CompareTo(items[parent]) < 0)
            {
                T tmp = items[i];
                items[i] = items[parent];
                items[parent] = tmp;
                Upheap(items, parent, size);
            }
        }

        public void Enqueue(T i)
        {
            m_itemCount++;
            m_items[m_itemCount] = i;
            Upheap(m_items, m_itemCount, m_itemCount+1);
        }

        public T Dequeue()
        {
            T ret = m_items[1];
            m_items[1] = m_items[m_itemCount];
            --m_itemCount;
            Downheap(m_items, 1, m_itemCount + 1);
            return ret;
        }

        public void Touch(int i)
        {
            Upheap(m_items, i, m_itemCount + 1);
            Downheap(m_items, i, m_itemCount + 1);
        }
    }
}
