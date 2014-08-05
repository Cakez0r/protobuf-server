using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Utility
{
    public class Fiber
    {
        private ConcurrentExclusiveSchedulerPair m_schedulers = new ConcurrentExclusiveSchedulerPair();

        public Task<T> EnqueueAwait<T>(Func<Task<T>> f, bool exclusive = true)
        {
            Task<Task<T>> wrapper = new Task<Task<T>>(() =>
            {
                Task<T> inner = Task<T>.Run(f);
                inner.Wait();
                return inner;
            });

            Start(wrapper, exclusive);

            return wrapper.Unwrap();
        }

        public Task EnqueueAwait(Func<Task> f, bool exclusive = true)
        {
            Task<Task> wrapper = new Task<Task>(() =>
            {
                Task inner = Task.Run(f);
                inner.Wait();
                return inner;
            });

            Start(wrapper, exclusive);

            return wrapper.Unwrap();
        }

        public Task<T> Enqueue<T>(Func<T> f, bool exclusive = true)
        {
            Task<T> task = new Task<T>(f);

            Start(task, exclusive);

            return task;
        }

        public Task Enqueue(Action f, bool exclusive = true)
        {
            Task task = new Task(f);

            Start(task, exclusive);

            return task;
        }

        private void Start(Task t, bool exclusive)
        {
            t.Start(m_schedulers.ExclusiveScheduler);
        }
    }
}
