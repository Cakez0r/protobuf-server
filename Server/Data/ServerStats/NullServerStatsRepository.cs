using System.Collections.Generic;

namespace Data.Stats
{
    public class NullServerStatsRepository : IServerStatsRepository
    {
        private static readonly IReadOnlyDictionary<string, long> s_blankDictionary = new Dictionary<string, long>();

        public int CPUUsage { get; set; }

        public long TotalBytesIn { get; set; }

        public long TotalBytesOut { get; set; }

        public long TotalPacketsIn { get; set; }

        public long TotalPacketsOut { get; set; }

        public long BytesInPerSecond { get; set; }

        public long BytesOutPerSecond { get; set; }

        public long PacketsInPerSecond { get; set; }

        public long PacketsOutPerSecond { get; set; }

        public int OnlinePlayerCount { get; set; }

        public int WorldUpdateTime { get; set; }

        public IReadOnlyDictionary<string, long> ZoneUpdateTimes
        {
            get { return s_blankDictionary; }
            set { }
        }
    }
}
