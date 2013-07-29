using Data.Accounts;
using Data.NPCs;
using Data.Players;
using NLog;
using Server.Zones;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class World
    {
        private const int TARGET_UPDATE_TIME_MS = 50;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<int, PlayerPeer> m_players = new ConcurrentDictionary<int, PlayerPeer>();

        private Thread m_worldUpdateThread;
        private DateTime m_lastUpdateTime;

        private IAccountRepository m_accountRepository;
        private INPCRepository m_npcRepository;
        private IPlayerRepository m_playerRepository;

        private Dictionary<int, Zone> m_zones;

        public World(IAccountRepository accountRepository, INPCRepository npcRepository, IPlayerRepository playerRepository)
        {
            m_accountRepository = accountRepository;
            m_npcRepository = npcRepository;
            m_playerRepository = playerRepository;

            m_zones = BuildZones(m_npcRepository);

            m_worldUpdateThread = new Thread(WorldUpdate);
            m_worldUpdateThread.Start();
        }

        public void AcceptSocket(Socket sock)
        {
            sock.NoDelay = true;

            PlayerPeer p = new PlayerPeer(sock, m_accountRepository, m_npcRepository, m_zones);

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

        private Dictionary<int, Zone> BuildZones(INPCRepository npcRepository)
        {
            Dictionary<int, Zone> zones = new Dictionary<int, Zone>();
            zones.Add(0, new Zone(0, m_npcRepository));
            zones.Add(1, new Zone(1, m_npcRepository));
            return zones;
        }
    }
}
