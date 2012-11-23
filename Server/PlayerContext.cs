using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PlayerContext : NetPeer
    {
        public PlayerContext(Socket socket) : base(socket)
        {

        }

        protected override void DispatchPacket(object packet)
        {
            Send(packet);
        }

        public void Update(TimeSpan dt)
        {

        }
    }
}
