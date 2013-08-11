using Data.Accounts;
using Data.NPCs;
using Data.Players;
using Data.Stats;
using NLog;
using Server.NPC;
using Server.Zones;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class World
    {
        private const int TARGET_UPDATE_TIME_MS = 50;
        private const int STATS_UPDATE_INTERVAL_MS = 1000;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<int, PlayerPeer> m_players = new ConcurrentDictionary<int, PlayerPeer>();

        private Thread m_worldUpdateThread;
        private DateTime m_lastUpdateTime;

        private Thread m_statsThread;

        private IAccountRepository m_accountRepository;
        private INPCRepository m_npcRepository;
        private IPlayerRepository m_playerRepository;
        private IServerStatsRepository m_statsRepository;

        private NPCFactory m_npcFactory;

        private Dictionary<int, Zone> m_zones;

        private int m_lastWorldUpdateLength;

        public World(IAccountRepository accountRepository, INPCRepository npcRepository, IPlayerRepository playerRepository, IServerStatsRepository statsRepository)
        {
            m_accountRepository = accountRepository;
            m_npcRepository = npcRepository;
            m_playerRepository = playerRepository;
            m_statsRepository = statsRepository;

            m_npcFactory = new NPCFactory(npcRepository);

            m_zones = BuildZones(m_npcRepository);

            m_worldUpdateThread = new Thread(WorldUpdate);
            m_worldUpdateThread.Start();

            m_statsThread = new Thread(StatsUpdate);
            m_statsThread.Start();
        }

        public void AcceptSocket(Socket sock)
        {
            sock.NoDelay = true;

            PlayerPeer p = new PlayerPeer(sock, m_accountRepository, m_npcRepository, m_playerRepository, m_zones);

            //NOTE: Code here will block the AcceptSocket loop, so make sure it stays lean
            m_players[p.ID] = p;

            s_log.Info("[{0}] connected", p.ID);
        }

        private void WorldUpdate()
        {
            s_log.Info("World update thread started");

            m_lastUpdateTime = DateTime.Now;
            Stopwatch updateTimer = new Stopwatch();
            while (true)
            {
                updateTimer.Restart();
                foreach (Zone zone in m_zones.Values)
                {
                    zone.Update();
                }

                Parallel.ForEach(m_players, kvp =>
                {
                    PlayerPeer player = kvp.Value;
                    if (player.IsConnected)
                    {
                        //This schedules an update on the player's fiber - does not run synchronously
                        player.Update();
                    }
                    else
                    {
                        s_log.Info("[{0}] is disconnected and will be removed", player.ID);
                        player.Dispose();
                        PlayerPeer removedPlayer = default(PlayerPeer);
                        m_players.TryRemove(kvp.Key, out removedPlayer);
                    }
                });
                updateTimer.Stop();

                m_lastWorldUpdateLength = (int)updateTimer.ElapsedMilliseconds;
                int restTime = TARGET_UPDATE_TIME_MS - m_lastWorldUpdateLength;

                if (restTime < 0)
                {
                    s_log.Warn("World update ran into overtime by {0}ms", Math.Abs(restTime));
                    restTime = 0;
                }

                m_lastUpdateTime = DateTime.Now;

                Thread.Sleep(restTime);
            }
        }

        private void StatsUpdate()
        {
            PerformanceCounter cpuCounter;

            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            long lastBytesIn = 0;
            long lastBytesOut = 0;
            long lastPacketsIn = 0;
            long lastPacketsOut = 0;

            while (true)
            {
                m_statsRepository.CPUUsage = (int)cpuCounter.NextValue();

                long bytesIn = NetPeer.TotalBytesIn;
                long bytesOut = NetPeer.TotalBytesOut;
                long packetsIn = NetPeer.TotalPacketsIn;
                long packetsOut = NetPeer.TotalPacketsOut;

                m_statsRepository.TotalBytesIn = bytesIn;
                m_statsRepository.TotalBytesOut = bytesOut;
                m_statsRepository.TotalPacketsIn = packetsIn;
                m_statsRepository.TotalPacketsOut = packetsOut;

                m_statsRepository.BytesInPerSecond = bytesIn - lastBytesIn;
                m_statsRepository.BytesOutPerSecond = bytesOut - lastBytesOut;
                m_statsRepository.PacketsInPerSecond = packetsIn - lastPacketsIn;
                m_statsRepository.PacketsOutPerSecond = packetsOut - lastPacketsOut;

                m_statsRepository.WorldUpdateTime = m_lastWorldUpdateLength;

                lastBytesIn = bytesIn;
                lastBytesOut = bytesOut;
                lastPacketsIn = packetsIn;
                lastPacketsOut = packetsOut;

                m_statsRepository.OnlinePlayerCount = m_players.Count;

                m_statsRepository.ZoneUpdateTimes = m_zones.Values.ToDictionary(z => "Zone " + z.ID, z => z.LastUpdateLength);

                Thread.Sleep(STATS_UPDATE_INTERVAL_MS);
            }
        }

        private Dictionary<int, Zone> BuildZones(INPCRepository npcRepository)
        {
            Dictionary<int, Zone> zones = new Dictionary<int, Zone>();
            zones.Add(0, new Zone(0, m_npcRepository, m_npcFactory));
            zones.Add(1, new Zone(1, m_npcRepository, m_npcFactory));
            return zones;
        }
    }
}
