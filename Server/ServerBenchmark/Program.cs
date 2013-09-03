using Protocol;
using System;
using System.Net.Sockets;
using System.Threading;

namespace ServerBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            ProtocolUtility.InitialiseSerializer();

            while (true)
            {
                for (int i = 0; i < 200; i++)
                {
                    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    s.Connect("127.0.0.1", 25012);
                    TestPeer tp = new TestPeer(s);
                    tp.Send(new AuthenticationAttempt_C2S() { Username = "Bot" + i, Password = "password123" });
                    Thread.Sleep(7);
                }
                Console.ReadKey();
            }
        }
    }
}
