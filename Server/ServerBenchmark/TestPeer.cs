using Protocol;
using Server;
using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerBenchmark
{
    public class TestPeer : NetPeer
    {
        private const float SPEED = 8;

        private Random m_rand;
        private PlayerStateUpdate_C2S m_psu;

        private Vector2 m_target;

        private Vector2 m_min = new Vector2(2400, 2400);
        private Vector2 m_max = new Vector2(2600, 2600);

        private Vector2 m_position;

        public TestPeer(Socket s) : base(s)
        {
            m_rand = new Random(GetHashCode());

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
            Fiber.Schedule(Update, TimeSpan.FromMilliseconds(50), true);
        }

        private Vector2 GetRandomTarget()
        {
            return new Vector2((ushort)m_rand.Next((int)m_min.X, (int)m_max.X), (ushort)m_rand.Next((int)m_min.Y, (int)m_max.Y));
        }
    }
}
