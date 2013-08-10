using BookSleeve;
using System.Configuration;
using System.Threading.Tasks;

namespace Data
{
    public class RedisRepository
    {
        protected RedisConnection Redis { get; private set; }
        
        public RedisRepository()
        {
            string host = ConfigurationManager.AppSettings["RedisHost"];
            Connect(host);
        }

        public RedisRepository(string host)
        {
            Connect(host);
        }

        private void Connect(string host)
        {
            Redis = new RedisConnection(host);
            Task connectionTask = Redis.Open();
            connectionTask.Wait(1000);
            if (connectionTask.Status != TaskStatus.RanToCompletion)
            {
                throw new RedisException("Failed to connect to Redis in time.");
            }
        }

        protected T SynchronousResult<T>(Task<T> t)
        {
            t.Wait();
            return t.Result;
        }
    }
}
