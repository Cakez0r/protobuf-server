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

namespace Server
{
    public class World
    {
        private const int TARGET_UPDATE_TIME_MS = 50;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private List<PlayerContext> m_players = new List<PlayerContext>();
        private ReaderWriterLockSlim m_playersLock = new ReaderWriterLockSlim();

        private Thread m_worldUpdateThread;
        private DateTime m_lastUpdateTime;

        private ZoneManager m_zoneManager = new ZoneManager();

        private Random m_rng = new Random((int)DateTime.Now.Ticks);

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
            m_playersLock.EnterWriteLock();
            m_players.Add(p);
            m_playersLock.ExitWriteLock();

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
                m_playersLock.EnterReadLock();

                Parallel.ForEach(m_players, p =>
                    {
                        p.Update(dt);
                        if (!p.IsConnected)
                        {
                            s_log.Info("{0} is disconnected and will be removed", p.Name);
                            p.DisconnectCleanup();
                            p.Dispose();
                        }
                    });
                m_playersLock.ExitReadLock();

                m_playersLock.EnterWriteLock();
                m_players.RemoveAll(p => p.Disposed);
                m_playersLock.ExitWriteLock();

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
            long lastMessagesSent = 0;
            long lastMessagesReceived = 0;

            while (true)
            {
                m_playersLock.EnterReadLock();
                long sent = m_players.Select(p => p.Stats.MessagesSent).Sum();
                long received = m_players.Select(p => p.Stats.MessagesReceived).Sum();

                Console.Title = "Players: " + m_players.Count() + " - In/Sec: " + (received - lastMessagesReceived) + " - Out/Sec " + (sent - lastMessagesSent);
                m_playersLock.ExitReadLock();

                lastMessagesSent = sent;
                lastMessagesReceived = received;

                Thread.Sleep(1000);
            }
        }
    }
}
