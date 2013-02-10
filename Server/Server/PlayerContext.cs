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
using Data;
using Data.Models;

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
            private set;
        }

        public bool IsAuthenticated
        {
            get { return m_account != null; }
        }

        public string Name
        {
            get;
            private set; 
        }

        private ReaderWriterLockSlim m_introductionLock = new ReaderWriterLockSlim();
        private HashSet<int> m_hasBeenIntroducedTo = new HashSet<int>();

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

        private ObjectRouter m_unauthenticatedHandler = new ObjectRouter();
        private ObjectRouter m_authenticatedHandler = new ObjectRouter();

        private AccountModel m_account;

        private static AccountRepository s_accountRepository = new AccountRepository();

        public PlayerContext(Socket socket, ZoneManager zoneManager) : base(socket)
        {
            ID = s_nextID++;
            Name = "Player " + ID;

            PlayerState = new PlayerStateUpdate_S2C();
            
            PlayerState.PlayerID = ID;
            PlayerState.MaxHP = 100;
            PlayerState.CurrentHP = 100;

            m_zoneManager = zoneManager;

            InitialiseRoutes();
        }

        private void InitialiseRoutes()
        {
            m_unauthenticatedHandler.SetRoute<AuthenticationAttempt_C2S>(Handle_AuthenticationAttempt);

            m_authenticatedHandler.SetRoute<PlayerStateUpdate_C2S>(Handle_PlayerStateUpdate);
            m_authenticatedHandler.SetRoute<ChatMessage>(Handle_ChatMessage);
        }

        public void Update(TimeSpan dt)
        {
            if (IsAuthenticated)
            {
                lock (m_worldState)
                {
                    Send(m_worldState);
                    m_worldState.PlayerStates.Clear();
                }
            }
        }

        protected override void DispatchPacket(Packet packet)
        {
            bool handled = IsAuthenticated ?
                m_authenticatedHandler.Route(packet) :
                m_unauthenticatedHandler.Route(packet);

            if (!handled)
            {
                s_log.Warn("Failed to handle packet of type {0}. Authenticated: {1} Name: ", packet.GetType(), IsAuthenticated, Name);
                Disconnect();
            }
        }

        public void IncludeInWorldState(PlayerContext player)
        {
            PlayerStateUpdate_S2C psu = player.HasBeenIntroducedTo(ID) ? player.PlayerState :
                new PlayerStateUpdate_S2C()
                {
                    PlayerID = player.ID,
                    X = player.PlayerState.X,
                    Y = player.PlayerState.Y,
                    Rot = player.PlayerState.Rot,
                    Introduction = player.GetIntroductionFor(ID),
                    TargetID = player.PlayerState.TargetID,
                    CurrentHP = player.PlayerState.CurrentHP,
                    MaxHP = player.PlayerState.MaxHP
                };
            
            lock (m_worldState)
            {
                m_worldState.PlayerStates.Add(psu);
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

        public bool HasBeenIntroducedTo(int id)
        {
            m_introductionLock.EnterReadLock();
            bool introduced = m_hasBeenIntroducedTo.Contains(id);
            m_introductionLock.ExitReadLock();

            return introduced;
        }

        public PlayerIntroduction GetIntroductionFor(int id)
        {
            m_introductionLock.EnterWriteLock();
            m_hasBeenIntroducedTo.Add(id);
            m_introductionLock.ExitWriteLock();

            return new PlayerIntroduction() { Name = Name };
        }

        private void Handle_PlayerStateUpdate(PlayerStateUpdate_C2S psu)
        {
            PlayerState.X = psu.X;
            PlayerState.Y = psu.Y;
            PlayerState.Rot = psu.Rot;

            if (psu.TargetID != PlayerState.TargetID)
            {
                s_log.Trace("{0} is now targetting {1}", Name, psu.TargetID == null ? "[Nothing]" : Global.World.GetPlayerByID(psu.TargetID.Value).Name);
            }
            PlayerState.TargetID = psu.TargetID;
        }

        private void Handle_ChatMessage(ChatMessage cm)
        {
            cm.SenderID = ID;
            CurrentZone.SendToAllInZone(cm);
            s_log.Info("{0} send to zone {1}: {2}", Name, CurrentZone.ID, cm.Message);
        }

        private void Handle_AuthenticationAttempt(AuthenticationAttempt_C2S aa)
        {
            m_account = s_accountRepository.GetWithLogin(aa.Username, aa.Password);

            AuthenticationAttempt_S2C.ResponseCode result;
            if (m_account != null)
            {
                Name = m_account.Name;
                result = AuthenticationAttempt_S2C.ResponseCode.OK;
                s_log.Info("Player {0} authenticated as {1}.", ID, m_account.Name);
            }
            else
            {
                result = AuthenticationAttempt_S2C.ResponseCode.BadLogin;
                s_log.Info("Player {0} failed to authenticate. Username: {1} Password: {2}", ID, aa.Username, aa.Password);
            }

            Respond(aa, new AuthenticationAttempt_S2C() { Result = result, PlayerID = ID });
        }
    }
}
