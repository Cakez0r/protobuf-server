using System;
using System.Collections.Generic;

namespace Server.Utility
{
    public class ObjectRouter
    {
        private Dictionary<Type, Action<object>> m_handlers = new Dictionary<Type, Action<object>>();

        public void SetRoute<T>(Action<T> handler)
        {
            if (!m_handlers.ContainsKey(typeof(T)))
            {
                m_handlers.Add(typeof(T), null);
            }

            m_handlers[typeof(T)] = (obj) => handler((T)obj);
        }

        public bool Route(object obj)
        {
            Action<object> handler = null;
            if (m_handlers.TryGetValue(obj.GetType(), out handler))
            {
                handler(obj);
                return true;
            }

            return false;
        }
    }
}
