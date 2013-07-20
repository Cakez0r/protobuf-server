using Data.Accounts;
using NLog;
using Protocol;
using Server.Utility;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public partial class PlayerPeer : NetPeer
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private ObjectRouter m_unauthenticatedHandler = new ObjectRouter();
        private ObjectRouter m_authenticatedHandler = new ObjectRouter();

        private int m_lastActivity = Environment.TickCount;
        private const int PING_TIMEOUT = 5000;

        private PlayerState m_state = new PlayerState();

        public bool IsAuthenticated
        {
            get;
            private set;
        }

        public PlayerPeer(Socket socket, IAccountRepository accountRepository) : base(socket)
        {
            m_accountRepository = accountRepository;

            InitialiseRoutes();
        }

        private void InitialiseRoutes()
        {
            m_unauthenticatedHandler.SetRoute<AuthenticationAttempt_C2S>(Handle_AuthenticationAttempt);
            m_unauthenticatedHandler.SetRoute<TimeSync_C2S>(Handle_TimeSync);

            m_authenticatedHandler.SetRoute<TimeSync_C2S>(Handle_TimeSync);
            m_authenticatedHandler.SetRoute<PlayerStateUpdate_C2S>(Handle_PlayerStateUpdate);
        }

        public void Update()
        {
            EnqueueWork(InternalUpdate);
        }

        private Future<T> SafeAccessState<T>(Func<PlayerState, T> accessor)
        {
            Future<T> future = new Future<T>();

            T synchronousResult = default(T);
            if (Monitor.TryEnter(m_state))
            {
                synchronousResult = accessor(m_state);
                Monitor.Exit(m_state);

                future.SetResult(synchronousResult);
            }
            else
            {
                EnqueueWork(() =>
                {
                    T result = default(T);
                    lock (m_state)
                    {
                        result = accessor(m_state);
                    }
                    future.SetResult(result);
                });
            }

            return future;
        }

        private void SafeAccessState(Action<PlayerState> accessor)
        {
            if (Monitor.TryEnter(m_state))
            {
                accessor(m_state);
                Monitor.Exit(m_state);
            }
            else
            {
                EnqueueWork(() =>
                {
                    lock (m_state)
                    {
                        accessor(m_state);
                    }
                });
            }
        }

        private void InternalUpdate()
        {
            if (Environment.TickCount - m_lastActivity > PING_TIMEOUT)
            {
                Send(new Ping());
                m_lastActivity = Environment.TickCount;
            }
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

        private void Handle_TimeSync(TimeSync_C2S sync)
        {
            s_log.Trace("Time sync request from ID {0}", ID);
            Respond(sync, new TimeSync_S2C() { Time = Environment.TickCount });
        }
    }
}
