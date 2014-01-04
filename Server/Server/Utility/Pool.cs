using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Utility
{

    public class Pool<T> where T : class, new()
    {
        private ConcurrentBag<T> m_pool = new ConcurrentBag<T>();

        public T Take()
        {
            T obj = default(T);
            m_pool.TryTake(out obj);

            return obj ?? new T();
        }

        public void Return(T obj)
        {
            m_pool.Add(obj);
        }
    }
}
