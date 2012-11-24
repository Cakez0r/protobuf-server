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

            while (true)
            {
                Socket socket = listener.AcceptSocket();
                s_log.Info("Player connected.");
                PlayerContext playerContext = new PlayerContext(socket);
                s_world.AcceptPlayer(playerContext);
            }
        }
    }
}
