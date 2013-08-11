using System.Collections.Generic;

namespace Data.Stats
{
    public interface IServerStatsRepository
    {
        int CPUUsage { get; set; }

        long TotalBytesIn { get; set; }
        long TotalBytesOut { get; set; }

        long TotalPacketsIn { get; set; }
        long TotalPacketsOut { get; set; }

        long BytesInPerSecond { get; set; }
        long BytesOutPerSecond { get; set; }
        
        long PacketsInPerSecond { get; set; }
        long PacketsOutPerSecond { get; set; }

        int OnlinePlayerCount { get; set; }

        int WorldUpdateTime { get; set; }

        IReadOnlyDictionary<string, long> ZoneUpdateTimes { get; set; }
    }
}
