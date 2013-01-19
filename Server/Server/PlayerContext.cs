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
using System.Threading;

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

        private ReaderWriterLockSlim m_zoneLock = new ReaderWriterLockSlim();
        private Zone m_currentZone;
        public Zone CurrentZone
        {
            get 
            {
                Zone currentZone = null;

                m_zoneLock.EnterReadLock();
                currentZone = m_currentZone;
                m_zoneLock.ExitReadLock();

                return currentZone; 
            }

            private set 
            {
                m_zoneLock.EnterWriteLock();
                m_currentZone = value;
                m_zoneLock.ExitWriteLock();
            }
        }

        private ZoneManager m_zoneManager;

        public PlayerContext(Socket socket, ZoneManager zoneManager) : base(socket)
        {
            ID = s_nextID++;
            PlayerState = new PlayerStateUpdate_S2C();
            PlayerState.ID = ID;
            m_zoneManager = zoneManager;
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
            else if (packet is ChatMessage)
            {
                ChatMessage cm = (ChatMessage)packet;
                cm.SenderID = ID;
                CurrentZone.SendToAllInZone(packet);
                s_log.Info("Player {0} send to zone {1}: {2}", cm.SenderID, CurrentZone.ID, cm.Message);
            }
        }

        public void IncludeInWorldState(PlayerContext player)
        {
            lock (m_worldState)
            {
                m_worldState.PlayerStates.Add(player.PlayerState);
            }
        }

        public void DisconnectCleanup()
        {
            m_zoneLock.EnterWriteLock();
            if (m_currentZone != null)
            {
                m_currentZone.RemovePlayer(this);
            }
            m_zoneLock.ExitWriteLock();
        }

        public void SwitchZone(int newZoneID)
        {
            m_zoneLock.EnterWriteLock();
            if (m_currentZone != null)
            {
                m_currentZone.RemovePlayer(this);
            }
            Zone newZone = m_zoneManager.GetZone(newZoneID);
            newZone.AddPlayer(this);
            m_currentZone = newZone;
            m_zoneLock.ExitWriteLock();
        }
    }
}
