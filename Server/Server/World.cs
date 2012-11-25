using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Server.Utility;

namespace Server
{
    public class World
    {
        private const int TARGET_UPDATE_TIME_MS = 50;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private List<PlayerContext> m_players = new List<PlayerContext>();
        private ReaderWriterLockSlim m_playersLock = new ReaderWriterLockSlim();

        private Thread m_worldUpdateThread;

        static World()
        {
        }

        public World()
        {
            m_worldUpdateThread = new Thread(WorldUpdate);
            m_worldUpdateThread.Start();

            new Thread(StatsThread).Start();
        }

        public void AcceptPlayer(PlayerContext p)
        {
            //NOTE: Code here will block the AcceptSocket loop, so make sure it stays lean
            m_playersLock.EnterWriteLock();
            m_players.Add(p);
            m_playersLock.ExitWriteLock();
            s_log.Info("Player {0} connected", p.PlayerState.ID);
        }

        private void WorldUpdate()
        {
            while (true)
            {
                Stopwatch updateTimer = Stopwatch.StartNew();

                m_playersLock.EnterReadLock();
                Parallel.ForEach(m_players, p1 =>
                    {
                        p1.Update(TimeSpan.Zero);
                        if (!p1.IsConnected)
                        {
                            s_log.Info("Player {0} is disconnected and will be removed", p1.PlayerState.ID);
                            p1.Dispose();
                        }

                        Parallel.ForEach(m_players, p2 =>
                            {
                                if (p1 != p2)
                                {
                                    if (Vector2.Distance(new Vector2(p1.PlayerState.X, p1.PlayerState.Y), new Vector2(p2.PlayerState.X, p2.PlayerState.Y)) < 25)
                                    {
                                        p1.IncludeInWorldState(p2);
                                    }
                                }
                            });
                    });
                m_playersLock.ExitReadLock();

                m_playersLock.EnterWriteLock();
                m_players.RemoveAll(p => !p.IsConnected);
                m_playersLock.ExitWriteLock();

                updateTimer.Stop();

                int restTime = TARGET_UPDATE_TIME_MS - (int)updateTimer.ElapsedMilliseconds;

                if (restTime < 0)
                {
                    s_log.Warn("World update ran into overtime by {0}ms", Math.Abs(restTime));
                    restTime = 0;
                }

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
                long sent = m_players.Select(p => p.Stats.BytesSent).Sum();
                long received = m_players.Select(p => p.Stats.BytesReceived).Sum();

                Console.Title = "Players: " + m_players.Count() + " - In/Sec: " + (received - lastMessagesReceived) + " - Out/Sec " + (sent - lastMessagesSent);
                m_playersLock.ExitReadLock();

                lastMessagesSent = sent;
                lastMessagesReceived = received;

                Thread.Sleep(1000);
            }
        }
    }
}
