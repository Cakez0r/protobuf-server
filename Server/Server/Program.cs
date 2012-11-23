using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NLog;
using System.Linq;

namespace Server
{
    class Program
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static World s_world = new World();

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 25012);
            listener.Start();
            s_log.Info("Started!");

            new Thread(StatsThread).Start();

            while (true)
            {
                Socket socket = listener.AcceptSocket();
                s_log.Info("Player connected.");
                PlayerContext playerContext = new PlayerContext(socket);
                s_world.AcceptPlayer(playerContext);
            }
        }

        static void StatsThread()
        {
            long lastMessagesSent = 0;
            long lastMessagesReceived = 0;

            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    long sent = s_world.m_players.Select(p => p.Stats.MessagedSent).Sum();
                    long received = s_world.m_players.Select(p => p.Stats.MessagedReceived).Sum();

                    Console.Title = "Players: " + s_world.m_players.Count() + " - In/Sec: " + (received - lastMessagesReceived) + " - Out/Sec " + (sent - lastMessagesSent);

                    lastMessagesSent = sent;
                    lastMessagesReceived = received;
                }
                catch (Exception ex)
                {

                }
            }
        }

    }
}
