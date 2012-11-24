using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using System.Linq;

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
        }

        private void WorldUpdate()
        {
            while (true)
            {
                Stopwatch updateTimer = Stopwatch.StartNew();

                m_playersLock.EnterReadLock();
                Parallel.ForEach(m_players, p =>
                    {
                        p.Update(TimeSpan.Zero);
                        if (!p.IsConnected)
                        {
                            s_log.Info("Player is disconnected and will be removed.");
                            p.Dispose();
                        }
                    });
                m_playersLock.ExitReadLock();

                m_playersLock.EnterWriteLock();
                m_players.RemoveAll(p => !p.IsConnected);
                m_playersLock.ExitWriteLock();

                updateTimer.Stop();

                int restTime = TARGET_UPDATE_TIME_MS - (int)updateTimer.ElapsedMilliseconds;

                if (restTime < 0)
                {
                    s_log.Warn("World update ran into overtime by {0}ms.", Math.Abs(restTime));
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
                long sent = m_players.Select(p => p.Stats.MessagedSent).Sum();
                long received = m_players.Select(p => p.Stats.MessagedReceived).Sum();

                Console.Title = "Players: " + m_players.Count() + " - In/Sec: " + (received - lastMessagesReceived) + " - Out/Sec " + (sent - lastMessagesSent);
                m_playersLock.ExitReadLock();

                lastMessagesSent = sent;
                lastMessagesReceived = received;

                Thread.Sleep(1000);
            }
        }
    }
}
