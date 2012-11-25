using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Protocol;
using Server.Utility;
using Server.Zones;

namespace Server
{
    public class PlayerContext : NetPeer
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static int s_nextID = 1;

        public int ID
        {
            get;
            private set;
        }

        public PlayerStateUpdate_S2C PlayerState
        {
            get;
            set;
        }

        private WorldState m_worldState = new WorldState() { PlayerStates = new List<PlayerStateUpdate_S2C>() };

        public PlayerContext(Socket socket) : base(socket)
        {
            ID = s_nextID++;
            PlayerState = new PlayerStateUpdate_S2C();
            PlayerState.ID = ID;
        }

        public void Update(TimeSpan dt)
        {
            lock (m_worldState)
            {
                Send(m_worldState);
                m_worldState.PlayerStates.Clear();
            }
        }

        protected override void DispatchPacket(object packet)
        {
            //TODO: Write some dispatcher...
            if (packet is PlayerStateUpdate_C2S)
            {
                PlayerStateUpdate_C2S psu = (PlayerStateUpdate_C2S)packet;
                PlayerState.X = psu.X;
                PlayerState.Y = psu.Y;
                PlayerState.Rot = psu.Rot;
            }
        }

        public void IncludeInWorldState(PlayerContext player)
        {
            lock (m_worldState)
            {
                m_worldState.PlayerStates.Add(player.PlayerState);
            }
        }
    }
}
