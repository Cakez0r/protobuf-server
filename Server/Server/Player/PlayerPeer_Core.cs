using Data.Abilities;
using Data.Accounts;
using Data.NPCs;
using Data.Players;
using NLog;
using Protocol;
using Server.Abilities;
using Server.Utility;
using Server.Zones;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Server
{
    public partial class PlayerPeer : NetPeer, ITargetable
    {
        private const int TARGET_UPDATE_TIME_MS = 50;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private ObjectRouter m_unauthenticatedHandler = new ObjectRouter();
        private ObjectRouter m_authenticatedHandler = new ObjectRouter();

        private int m_lastActivity = Environment.TickCount;
        private const int PING_TIMEOUT = 5000;

        public bool IsAuthenticated
        {
            get;
            private set;
        }

        public PlayerPeer(Socket socket, IAccountRepository accountRepository, INPCRepository npcRepository, IPlayerRepository playerRepository, IAbilityRepository abilityRepository, Dictionary<int, Zone> zones) : base(socket)
        {
            m_accountRepository = accountRepository;
            m_playerRepository = playerRepository;
            m_npcRepository = npcRepository;
            m_abilityRepository = abilityRepository;
            m_zones = zones;

            InitialiseRoutes();

            Fiber.Enqueue(Update);
        }

        private void InitialiseRoutes()
        {
            m_unauthenticatedHandler.SetRoute<AuthenticationAttempt_C2S>(Handle_AuthenticationAttempt);
            m_unauthenticatedHandler.SetRoute<TimeSync_C2S>(Handle_TimeSync);

            m_authenticatedHandler.SetRoute<TimeSync_C2S>(Handle_TimeSync);
            m_authenticatedHandler.SetRoute<PlayerStateUpdate_C2S>(Handle_PlayerStateUpdate);
            m_authenticatedHandler.SetRoute<UseAbility_C2S>(Handle_UseAbility);
        }

        private void Update()
        {
            LatestStateUpdate = new PlayerStateUpdate_S2C()
            {
                PlayerID = ID,
                CurrentHP = 0,
                MaxHP = 0,
                Rot = Rotation,
                TargetID = TargetID,
                Time = TimeOnClient,
                VelX = Velocity.X,
                VelY = Velocity.Y,
                X = Position.X,
                Y = Position.Y
            };

            if (IsAuthenticated && CurrentZone != null)
            {
                BuildAndSendWorldStateUpdate();
            }

            if (Environment.TickCount - m_lastActivity > PING_TIMEOUT)
            {
                Send(new Ping());
                m_lastActivity = Environment.TickCount;
            }

            Fiber.Schedule(Update, TimeSpan.FromMilliseconds(TARGET_UPDATE_TIME_MS));
        }

        protected override void DispatchPacket(Packet packet)
        {
            bool handled = IsAuthenticated ?
                m_authenticatedHandler.Route(packet) :
                m_unauthenticatedHandler.Route(packet);

            if (!handled)
            {
                s_log.Warn("Failed to handle packet of type {0}. Authenticated: {1}", packet.GetType(), IsAuthenticated);
                Disconnect();
            }

            m_lastActivity = Environment.TickCount;
        }

        public override void Dispose()
        {
            try
            {
                if (CurrentZone != null)
                {
                    CurrentZone.RemoveFromZone(this);
                }
            }
            catch (Exception ex)
            {
                s_log.Warn("Failed to dispose player: {0}", ex);
            }

            base.Dispose();
        }

        private void Handle_TimeSync(TimeSync_C2S sync)
        {
            s_log.Trace("Time sync request from ID {0}", ID);
            Respond(sync, new TimeSync_S2C() { Time = Environment.TickCount });
        }
    }
}
