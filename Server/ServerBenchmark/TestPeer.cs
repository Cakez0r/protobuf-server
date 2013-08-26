using Protocol;
using Server;
using Server.Utility;
using System;
using System.Net.Sockets;

namespace ServerBenchmark
{
    public class TestPeer : NetPeer
    {
        private const float SPEED = 8;

        private Random m_rand;
        private PlayerStateUpdate_C2S m_psu;

        private Vector2 m_target;

        private Vector2 m_min = new Vector2(2000, 2000);
        private Vector2 m_max = new Vector2(3000, 3000);

        private Vector2 m_position;

        public TestPeer(Socket s) : base(s)
        {
            m_rand = new Random(s.GetHashCode());

            m_position = GetRandomTarget();

            m_psu = new PlayerStateUpdate_C2S()
            {
                Rot = 0,
                Time = Environment.TickCount,
                VelX = 0,
                VelY = 0,
                X = Compression.PositionToUShort(m_position.X),
                Y = Compression.PositionToUShort(m_position.Y)
            };
        }

        protected override void DispatchPacket(Packet packet)
        {
            if (packet is AuthenticationAttempt_S2C)
            {
                AuthenticationAttempt_S2C auth = (AuthenticationAttempt_S2C)packet;
                if (auth.Result == AuthenticationAttempt_S2C.ResponseCode.OK)
                {
                    m_target = GetRandomTarget();
                    Update();
                }
            }
        }

        private void Update()
        {
            Vector2 delta = m_target - m_position;
            if (delta.LengthSquared() < 8)
            {
                m_target = GetRandomTarget();
            }

            delta = m_target - m_position;
            delta.Normalize();
            delta *= SPEED;

            Vector2 velocity = delta;

            m_position += delta * (1.0f / 20);

            m_psu.VelX = Compression.VelocityToShort(velocity.X);
            m_psu.VelY = Compression.VelocityToShort(velocity.Y);

            m_psu.X = Compression.PositionToUShort(m_position.X);
            m_psu.Y = Compression.PositionToUShort(m_position.Y);

            m_psu.Time = Environment.TickCount;

            Send(m_psu);

            if (IsConnected)
            {
                Fiber.Schedule(Update, TimeSpan.FromMilliseconds(50), true);
            }
        }

        private Vector2 GetRandomTarget()
        {
            return new Vector2((ushort)m_rand.Next((int)m_min.X, (int)m_max.X), (ushort)m_rand.Next((int)m_min.Y, (int)m_max.Y));
        }
    }
}
