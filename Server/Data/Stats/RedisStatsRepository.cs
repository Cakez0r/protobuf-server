using BookSleeve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Stats
{
    public class RedisStatsRepository : RedisRepository, IStatsRepository
    {
        private const int DB_NUMBER = 1;

        private const string TOTAL_STATS_HASHNAME = "TotalStats";
        private const string ZONE_UPDATE_TIMES_HASHNAME = "ZoneUpdateTimes";

        public int CPUUsage
        {
            get
            {
                return (int)SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "CPUUsage"));
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "CPUUsage", value.ToString());
            }
        }

        public long TotalBytesIn
        {
            get
            {
                return SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "TotalBytesIn")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "TotalBytesIn", value.ToString());
            }
        }

        public long TotalBytesOut
        {
            get
            {
                return SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "TotalBytesOut")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "TotalBytesOut", value.ToString());
            }
        }

        public long TotalPacketsIn
        {
            get
            {
                return SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "TotalPacketsIn")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "TotalPacketsIn", value.ToString());
            }
        }

        public long TotalPacketsOut
        {
            get
            {
                return SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "TotalPacketsOut")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "TotalPacketsOut", value.ToString());
            }
        }

        public long BytesInPerSecond
        {
            get
            {
                return SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "BytesInPerSecond")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "BytesInPerSecond", value.ToString());
            }
        }

        public long BytesOutPerSecond
        {
            get
            {
                return SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "BytesOutPerSecond")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "BytesOutPerSecond", value.ToString());
            }
        }

        public long PacketsInPerSecond
        {
            get
            {
                return SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "PacketsInPerSecond")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "PacketsInPerSecond", value.ToString());
            }
        }

        public long PacketsOutPerSecond
        {
            get
            {
                return SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "PacketsOutPerSecond")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "PacketsOutPerSecond", value.ToString());
            }
        }

        public int OnlinePlayerCount
        {
            get
            {
                return (int)SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "OnlinePlayerCount")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "OnlinePlayerCount", value.ToString());
            }
        }

        public int WorldUpdateTime
        {
            get
            {
                return (int)SynchronousResult(Redis.Hashes.GetInt64(DB_NUMBER, TOTAL_STATS_HASHNAME, "WorldUpdateTime")).Value;
            }
            set
            {
                Redis.Hashes.Set(DB_NUMBER, TOTAL_STATS_HASHNAME, "WorldUpdateTime", value.ToString());
            }
        }

        public IReadOnlyDictionary<string, long> ZoneUpdateTimes
        {
            get { return RedisHashToDictionary(SynchronousResult(Redis.Hashes.GetAll(DB_NUMBER, ZONE_UPDATE_TIMES_HASHNAME))); }
            set { Redis.Hashes.Set(DB_NUMBER, ZONE_UPDATE_TIMES_HASHNAME, DictionaryToRedisHash(value)); }
        }

        public RedisStatsRepository() { }

        public RedisStatsRepository(string host) : base(host) { }

        private IReadOnlyDictionary<string, long> RedisHashToDictionary(Dictionary<string, byte[]> redisDictionary)
        {
            return redisDictionary.ToDictionary(kvp => kvp.Key, kvp => BitConverter.ToInt64(kvp.Value, 0));
        }

        private Dictionary<string, byte[]> DictionaryToRedisHash(IReadOnlyDictionary<string, long> dictionary)
        {
            return dictionary.ToDictionary(kvp => kvp.Key, kvp => BitConverter.GetBytes(kvp.Value));
        }
    }
}
