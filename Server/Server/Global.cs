using System.Net;
using System.Net.Sockets;
using NLog;
using System;

namespace Server
{
    public class Global
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static World s_world = new World();
        public static World World
        {
            get { return s_world; }
        }

        private static void Main(string[] args)
        {
            Protocol.ProtocolUtility.InitialiseSerializer();

            //Start listening for connnections
            TcpListener listener = new TcpListener(IPAddress.Any, 25012);
            listener.Start();
            s_log.Info("Listening for connections on " + listener.LocalEndpoint.ToString());

            while (true)
            {
                Socket socket = listener.AcceptSocket();
                s_world.AcceptSocket(socket);
            }
        }
    }
}
