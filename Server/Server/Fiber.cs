using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NLog;

namespace Server
{
    public class Fiber
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        ActionBlock<Action> m_workQueue;

        public Fiber()
        {
            m_workQueue = new ActionBlock<Action>((a) => a());
        }

        public void Enqueue(Action a)
        {
            if (!m_workQueue.Post(a))
            {
                s_log.Warn("Failed to post work to a fiber.");
            }
        }
    }
}
