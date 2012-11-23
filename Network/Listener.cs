using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Network
{
    public class Listener
    {
        TcpListener m_listener = new TcpListener(IPAddress.Any, 25012);

        public Listener()
        {
            m_listener.Start();
        }

        public async Task<Socket> AcceptSocket()
        {
            Socket sock = await m_listener.AcceptSocketAsync();
            return sock;
        }
    }
}
