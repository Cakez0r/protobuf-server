using Data.Abilities;
using Data.Accounts;
using Data.Players;
using NLog;
using Protocol;
using Server.Abilities;
using Server.Utility;
using Server.Zones;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

namespace Server
{
    [DebuggerDisplay("ID: {ID}")]
    public partial class PlayerPeer : NetPeer, IEntity
    {
        private const int TARGET_UPDATE_TIME_MS = 50;
        private const int SAVE_INTERVAL_MS = 60000;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        [Flags]
        private enum SaveFlags : uint
        {
            General = 1,
            Stats = 2,
            All = 0xFFFFFFFF
        }

        private ObjectRouter m_unauthenticatedHandler = new ObjectRouter();
        private ObjectRouter m_authenticatedHandler = new ObjectRouter();
        private bool m_routesInitialised = false;

        private int m_lastActivity = Environment.TickCount;
        private const int PING_TIMEOUT = 5000;

        private int m_lastSaveTime = Environment.TickCount;

        public bool IsAuthenticated
        {
            get;
            private set;
        }

        public string Name
        {
            get;// { return m_player != null ? m_player.Name : string.Format("[Peer{0}]", ID); }
            set;
        }

        public PlayerPeer(Socket socket, IAccountRepository accountRepository, IPlayerRepository playerRepository, IAbilityRepository abilityRepository, Dictionary<int, Zone> zones)
            : base(socket)
        {
            m_accountRepository = accountRepository;
            m_playerRepository = playerRepository;
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
            m_authenticatedHandler.SetRoute<ChatMessage>(Handle_ChatMessage);
            m_authenticatedHandler.SetRoute<PlayerStateUpdate>(Handle_PlayerStateUpdate);
            m_authenticatedHandler.SetRoute<UseAbility_C2S>(Handle_UseAbility);
            m_authenticatedHandler.SetRoute<StopCasting>(Handle_StopCasting);

            m_routesInitialised = true;
        }

        private void Update()
        {
            if (IsAuthenticated)
            {
                m_latestStateUpdate = new EntityStateUpdate()
                {
                    ID = ID,
                    Health = (ushort)Health,
                    Power = (ushort)Power,
                    Rotation = Rotation,
                    TargetID = TargetID,
                    Timestamp = TimeOnClient,
                    VelX = m_compressedVelX,
                    VelY = m_compressedVelY,
                    X = m_compressedX,
                    Y = m_compressedY,
                    CastingEffect = m_lastAbility.State == AbilityState.Casting ? (ushort)m_lastAbility.Ability.AbilityID : (ushort)0
                };

                if (CurrentZone != null)
                {
                    BuildAndSendWorldStateUpdate();
                }
                if (Environment.TickCount - m_lastSaveTime > SAVE_INTERVAL_MS)
                {
                    Save(SaveFlags.General);
                }
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
            if (m_routesInitialised)
            {
                bool handled = IsAuthenticated ?
                    m_authenticatedHandler.Route(packet) :
                    m_unauthenticatedHandler.Route(packet);

                if (!handled)
                {
                    Warn("Failed to handle packet of type {0}", packet.GetType());
                    Disconnect();
                }

                m_lastActivity = Environment.TickCount;
            }
            else
            {
                Warn("Received packet before routes are initialised. Rescheduling it...");
                Fiber.Schedule(() => DispatchPacket(packet), TimeSpan.FromMilliseconds(50));
            }
        }

        protected override void Dispose(bool disposing)
        {
            Fiber.Enqueue(() =>
            {
                try
                {
                    Save(SaveFlags.All);

                    if (CurrentZone != null)
                    {
                        CurrentZone.RemoveFromZone(this);
                    }

                }
                catch (Exception ex)
                {
                    Warn("Failed to dispose player: {0}", ex);
                }

                base.Dispose(true);
            });

            bool loggedIn = false;
            s_loggedInAccounts.TryRemove(AccountID, out loggedIn);

            m_lastAbility.Dispose();
        }

        private void Save(SaveFlags saveFlags)
        {
            if (!IsAuthenticated || CurrentZone == null)
            {
                return;
            }

            if ((saveFlags & SaveFlags.General) == SaveFlags.General)
            {
                //Trace("Saving (General)");
                float health = (float)Health / MaxHealth;
                float power = (float)Power / MaxPower;
                m_playerRepository.UpdatePlayer(m_player.PlayerID, m_player.AccountID, m_player.Name, health, power, 0, CurrentZone.ID, Position.X, Position.Y, Rotation);
                m_lastSaveTime = Environment.TickCount;
            }

            if ((saveFlags & SaveFlags.Stats) == SaveFlags.Stats)
            {
                //Trace("Saving (Stats)");
                foreach (PlayerStatModel stat in m_stats.Values)
                {
                    m_playerRepository.UpdatePlayerStat(stat.PlayerStatID, stat.PlayerID, stat.StatID, stat.StatValue);
                }
            }
        }

        public void EnqueueSend(Packet p)
        {
            Fiber.Enqueue(() => Send(p));
        }

        private void Handle_TimeSync(TimeSync_C2S sync)
        {
            Respond(sync, new TimeSync_S2C() { Time = Environment.TickCount });
        }

        private void Handle_ChatMessage(ChatMessage cm)
        {
            Info("Chat: {0}", cm.Message);
            CurrentZone.SendMessageToZone(m_player.Name, cm.Message);
        }

        #region Logging
        private const string LOG_FORMAT = "[{0}] {1}: {2}";
        private void Trace(string message, params object[] args)
        {
            s_log.Trace(string.Format(LOG_FORMAT, ID, Name, string.Format(message, args)));
        }
        private void Info(string message, params object[] args)
        {
            s_log.Info(string.Format(LOG_FORMAT, ID, Name, string.Format(message, args)));
        }
        private void Warn(string message, params object[] args)
        {
            s_log.Warn(string.Format(LOG_FORMAT, ID, Name, string.Format(message, args)));
        }
        private void Error(string message, params object[] args)
        {
            s_log.Error(string.Format(LOG_FORMAT, ID, Name, string.Format(message, args)));
        }
        #endregion
    }
}
