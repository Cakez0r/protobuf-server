using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Server.Utility;
using Server.Zones;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace Server
{
    public class World
    {
        private const int TARGET_UPDATE_TIME_MS = 50;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<int, PlayerContext> m_players = new ConcurrentDictionary<int, PlayerContext>();

        private Thread m_worldUpdateThread;
        private DateTime m_lastUpdateTime;

        private ZoneManager m_zoneManager = new ZoneManager();

        private Random m_rng = new Random((int)DateTime.Now.Ticks);

        private List<int> m_disposedPlayerList = new List<int>();

        public int Clock
        {
            get { return Environment.TickCount; }
        }

        static World()
        {
        }

        public World()
        {
            m_worldUpdateThread = new Thread(WorldUpdate);
            m_worldUpdateThread.Start();

            new Thread(StatsThread).Start();
        }

        public void AcceptSocket(Socket sock)
        {
            sock.NoDelay = true;

            PlayerContext p = new PlayerContext(sock, m_zoneManager);

            //NOTE: Code here will block the AcceptSocket loop, so make sure it stays lean
            m_players[p.ID] = p;

            p.SwitchZone(0);

            s_log.Info("Player {0} connected", p.PlayerState.PlayerID);
        }

        private void WorldUpdate()
        {
            m_lastUpdateTime = DateTime.Now;

            while (true)
            {
                TimeSpan dt = DateTime.Now - m_lastUpdateTime;

                Stopwatch updateTimer = Stopwatch.StartNew();

                m_zoneManager.Update(dt);

                Parallel.ForEach(m_players, kvp =>
                {
                    PlayerContext player = kvp.Value;
                    player.Update(dt);
                    if (!player.IsConnected)
                    {
                        s_log.Info("{0} is disconnected and will be removed", player.Name);
                        player.DisconnectCleanup();
                        player.Dispose();
                        PlayerContext removedPlayer = default(PlayerContext);
                        m_players.TryRemove(kvp.Key, out removedPlayer);
                    }
                });

                updateTimer.Stop();

                int restTime = TARGET_UPDATE_TIME_MS - (int)updateTimer.ElapsedMilliseconds;

                if (restTime < 0)
                {
                    s_log.Warn("World update ran into overtime by {0}ms", Math.Abs(restTime));
                    restTime = 0;
                }

                m_lastUpdateTime = DateTime.Now;

                Thread.Sleep(restTime);
            }
        }

        private void StatsThread()
        {
            while (true)
            {
                Console.Title = "Players: " + m_players.Count();

                Thread.Sleep(1000);
            }
        }

        public PlayerContext GetPlayerByID(int id)
        {
            PlayerContext pc = default(PlayerContext);
            m_players.TryGetValue(id, out pc);
            
            return pc;
        }
    }
}
