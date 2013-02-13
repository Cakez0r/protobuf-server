using System;
using System.Threading.Tasks.Dataflow;
using NLog;

namespace Server.Utility
{
    public class Fiber
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private ActionBlock<Action> m_workQueue;

        public Fiber()
        {
            m_workQueue = new ActionBlock<Action>((a) => a());
        }

        public void Enqueue(Action a)
        {
            if (!m_workQueue.Post(a))
            {
                s_log.Warn("Failed to post work to a fiber. Work queue is full.");
            }
        }
    }
}
