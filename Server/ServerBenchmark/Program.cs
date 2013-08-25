using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            ProtocolUtility.InitialiseSerializer();

            for (int i = 0; i < 50; i++)
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                s.Connect("127.0.0.1", 25012);
                TestPeer tp = new TestPeer(s);
                tp.Send(new AuthenticationAttempt_C2S() { Username = "Bot" + i, Password = "password123" });
                Thread.Sleep(5);
            }
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
