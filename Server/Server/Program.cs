using System.Net;
using System.Net.Sockets;
using NLog;

namespace Server
{
    class Program
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static World s_world = new World();

        static void Main(string[] args)
        {
            Protocol.ProtocolUtility.InitialiseSerializer();

            TcpListener listener = new TcpListener(IPAddress.Any, 25012);
            listener.Start();
            s_log.Info("Started!");

            while (true)
            {
                Socket socket = listener.AcceptSocket();
                s_world.AcceptSocket(socket);
            }
        }
    }
}
