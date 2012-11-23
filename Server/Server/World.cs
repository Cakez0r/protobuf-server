using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Server
{
    public class World
    {
        private const int TARGET_UPDATE_TIME_MS = 50;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        public List<PlayerContext> m_players = new List<PlayerContext>();

        private Thread m_worldUpdateThread;

        static World()
        {
        }

        public World()
        {
            m_worldUpdateThread = new Thread(WorldUpdate);
            m_worldUpdateThread.Start();
        }

        public void AcceptPlayer(PlayerContext p)
        {
            //NOTE: Code here will block the AcceptSocket loop, so make sure it stays lean
            m_players.Add(p);
        }

        private void WorldUpdate()
        {
            while (true)
            {
                Stopwatch updateTimer = Stopwatch.StartNew();

                Parallel.ForEach(m_players, p =>
                    {
                        p.Update(TimeSpan.Zero);
                        if (!p.IsConnected)
                        {
                            s_log.Info("Player is disconnected and will be removed.");
                            p.Dispose();
                        }
                    });

                m_players.RemoveAll(p => !p.IsConnected);

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
    }
}
