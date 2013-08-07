using BookSleeve;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
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
            Redis.Open().Wait();
        }

        protected T SynchronousResult<T>(Task<T> t)
        {
            t.Wait();
            return t.Result;
        }
    }
}
