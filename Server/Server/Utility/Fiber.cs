using NLog;
using System;
using System.Threading.Tasks.Dataflow;

namespace Server.Utility
{
    public class Fiber
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        public int WorkQueueLength
        {
            get { return m_workQueue.InputCount; }
        }

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

        public void Stop()
        {
            m_workQueue.Complete();
        }
    }
}
