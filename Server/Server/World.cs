using Data.Abilities;
using Data.Accounts;
using Data.NPCs;
using Data.Players;
using Data.Stats;
using NLog;
using Server.NPC;
using Server.Utility;
using Server.Zones;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    public sealed class World : IDisposable
    {
        private const int TARGET_UPDATE_TIME_MS = 50;
        private const int STATS_UPDATE_INTERVAL_MS = 1000;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<int, PlayerPeer> m_players = new ConcurrentDictionary<int, PlayerPeer>();

        private Fiber m_fiber = new Fiber();

        private IAccountRepository m_accountRepository;
        private INPCRepository m_npcRepository;
        private IPlayerRepository m_playerRepository;
        private IServerStatsRepository m_statsRepository;
        private IAbilityRepository m_abilityRepository;

        private NPCFactory m_npcFactory;

        private Dictionary<int, Zone> m_zones;

        private int m_lastWorldUpdateLength;

        PerformanceCounter m_cpuCounter = new PerformanceCounter();

        private long m_lastBytesIn = 0;
        private long m_lastBytesOut = 0;
        private long m_lastPacketsIn = 0;
        private long m_lastPacketsOut = 0;

        public World(IAccountRepository accountRepository, INPCRepository npcRepository, IPlayerRepository playerRepository, IServerStatsRepository statsRepository, IAbilityRepository abilityRepository)
        {
            m_accountRepository = accountRepository;
            m_npcRepository = npcRepository;
            m_playerRepository = playerRepository;
            m_statsRepository = statsRepository;
            m_abilityRepository = abilityRepository;

            m_npcFactory = new NPCFactory(npcRepository);

            m_zones = BuildZones(m_npcRepository);

            m_cpuCounter.CategoryName = "Processor";
            m_cpuCounter.CounterName = "% Processor Time";
            m_cpuCounter.InstanceName = "_Total";

            m_fiber.Enqueue(Update, false);
            m_fiber.Enqueue(StatsUpdate, false);
        }

        public void AcceptSocket(Socket sock)
        {
            sock.NoDelay = true;

            PlayerPeer p = new PlayerPeer(sock, m_accountRepository, m_npcRepository, m_playerRepository, m_abilityRepository, m_zones);

            //NOTE: Code here will block the AcceptSocket loop, so make sure it stays lean
            m_players[p.ID] = p;

            s_log.Info("[{0}] connected", p.ID);
        }

        private void Update()
        {
            Stopwatch updateTimer = Stopwatch.StartNew();
            Parallel.ForEach(m_players, kvp =>
            {
                PlayerPeer player = kvp.Value;
                if (!player.IsConnected)
                {
                    s_log.Info("[{0}] is disconnected and will be removed", player.ID);
                    PlayerPeer.LoggedInAccounts[player.AccountID] = false;
                    player.Dispose();
                    PlayerPeer removedPlayer = default(PlayerPeer);
                    m_players.TryRemove(kvp.Key, out removedPlayer);
                }
            });
            updateTimer.Stop();

            m_lastWorldUpdateLength = (int)updateTimer.ElapsedMilliseconds;
            int restTime = TARGET_UPDATE_TIME_MS - m_lastWorldUpdateLength;

            if (restTime >= 0)
            {
                m_fiber.Schedule(Update, TimeSpan.FromMilliseconds(restTime), false);
            }
            else
            {
                s_log.Warn("World update ran into overtime by {0}ms", Math.Abs(restTime));
                m_fiber.Enqueue(Update, false);
            }
        }

        private void StatsUpdate()
        {
            m_statsRepository.CPUUsage = (int)m_cpuCounter.NextValue();

            long bytesIn = NetPeer.TotalBytesIn;
            long bytesOut = NetPeer.TotalBytesOut;
            long packetsIn = NetPeer.TotalPacketsIn;
            long packetsOut = NetPeer.TotalPacketsOut;

            m_statsRepository.TotalBytesIn = bytesIn;
            m_statsRepository.TotalBytesOut = bytesOut;
            m_statsRepository.TotalPacketsIn = packetsIn;
            m_statsRepository.TotalPacketsOut = packetsOut;

            m_statsRepository.BytesInPerSecond = bytesIn - m_lastBytesIn;
            m_statsRepository.BytesOutPerSecond = bytesOut - m_lastBytesOut;
            m_statsRepository.PacketsInPerSecond = packetsIn - m_lastPacketsIn;
            m_statsRepository.PacketsOutPerSecond = packetsOut - m_lastPacketsOut;

            m_statsRepository.WorldUpdateTime = m_lastWorldUpdateLength;

            m_lastBytesIn = bytesIn;
            m_lastBytesOut = bytesOut;
            m_lastPacketsIn = packetsIn;
            m_lastPacketsOut = packetsOut;

            m_statsRepository.OnlinePlayerCount = m_players.Count;

            m_statsRepository.ZoneUpdateTimes = m_zones.Values.ToDictionary(z => "Zone " + z.ID, z => z.LastUpdateLength);

            Console.Title = string.Format("Players Online: {0}", m_players.Count);

            m_fiber.Schedule(StatsUpdate, TimeSpan.FromMilliseconds(STATS_UPDATE_INTERVAL_MS), false);
        }

        private Dictionary<int, Zone> BuildZones(INPCRepository npcRepository)
        {
            Dictionary<int, Zone> zones = new Dictionary<int, Zone>();
            for (int i = 0; i < 10; i++)
            {
                zones.Add(i, new Zone(i, m_npcRepository, m_npcFactory));
            }
            return zones;
        }

        public void Dispose()
        {
            m_cpuCounter.Dispose();
        }
    }
}
