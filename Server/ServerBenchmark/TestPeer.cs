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
        private Random m_rand;
        private PlayerStateUpdate_C2S m_psu;

        private Vector2 m_target;

        public TestPeer(Socket s) : base(s)
        {
            m_rand = new Random(GetHashCode());
            m_psu = new PlayerStateUpdate_C2S()
            {
                Rot = 0,
                TargetID = m_rand.Next(),
                Time = Environment.TickCount,
                VelX = 0,
                VelY = 0,
                X = Compression.PositionToUShort(m_rand.Next(2400, 2600)),
                Y = Compression.PositionToUShort(m_rand.Next(2400, 2600))
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
            Send(m_psu);
            Fiber.Schedule(Update, TimeSpan.FromMilliseconds(50), true);
        }

        private Vector2 GetRandomTarget()
        {
            return new Vector2((ushort)m_rand.Next(0, ushort.MaxValue), (ushort)m_rand.Next(0, ushort.MaxValue));
        }
    }
}
