using Data.Accounts;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using NLog;
using Protocol;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            ServerConfiguration config = (ServerConfiguration)ConfigurationManager.GetSection("server");

            s_log.Info("Creating repositories...");
            UnityContainer repositoryResolver = new UnityContainer();
            repositoryResolver.LoadConfiguration();

            IAccountRepository accountRepository = repositoryResolver.Resolve<IAccountRepository>();
            s_log.Info("... done");


            s_log.Info("Creating world...");
            World world = new World(accountRepository);
            s_log.Info("... done");


            ProtocolUtility.InitialiseSerializer();

            TcpListener listener = new TcpListener(IPAddress.Any, config.Port);
            listener.Start();
            s_log.Info("Listening for connections on " + listener.LocalEndpoint.ToString());

            while (true)
            {
                Socket socket = listener.AcceptSocket();
                world.AcceptSocket(socket);
            }
        }
    }
}
