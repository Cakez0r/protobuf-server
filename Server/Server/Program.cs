using Data.Abilities;
using Data.Accounts;
using Data.NPCs;
using Data.Players;
using Data.Stats;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using NLog;
using Protocol;
using Server.Map;
using Server.Utility;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        public static MapData Map;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            Map = MapData.LoadFromFile("game2.unity.map");
            Bitmap bmp = Map.RenderMap(1024);
            bmp.Save("map.png");

            //{
            //    MapData map = MapData.LoadFromFile("bugged.map");
            //    Bitmap bmp = map.RenderMap(1024);
            //    bmp.Save("path.png");

            //    Random r = new Random(123432432);
            //    int[] rand = Enumerable.Range(0, 1000000).Select(i => r.Next(map.Waypoints.Length)).ToArray();
            //    Stopwatch sw = Stopwatch.StartNew();
            //    //Parallel.For(0, rand.Length - 4, (i) =>
            //    for (int i = 0; i < rand.Length / 2; i++)
            //    {
            //        Vector2 direction = map.GetDirection(new Vector2(rand[i], rand[i + 1]), new Vector2(rand[i + 2], rand[i + 3]));
            //    }//);
            //    sw.Stop();
            //    s_log.Info("Pathfinding took {0} ms", sw.ElapsedMilliseconds);
            //}

            ServerConfiguration config = (ServerConfiguration)ConfigurationManager.GetSection("server");

            s_log.Info("Creating repositories...");
            UnityContainer repositoryResolver = new UnityContainer();
            repositoryResolver.LoadConfiguration();

            IAccountRepository accountRepository = repositoryResolver.Resolve<IAccountRepository>();
            INPCRepository npcRepository = repositoryResolver.Resolve<INPCRepository>();
            IPlayerRepository playerRepository = repositoryResolver.Resolve<IPlayerRepository>();
            IAbilityRepository abilityRepository = repositoryResolver.Resolve<IAbilityRepository>();
            IServerStatsRepository statsRepository = new NullServerStatsRepository();
            try
            {
                statsRepository = repositoryResolver.Resolve<IServerStatsRepository>();
            }
            catch
            {
                s_log.Warn("Failed to create stats repository. Stats will be disabled.");
            }

            s_log.Info("Precaching NPCs...");
            var npcs = npcRepository.GetNPCs();

            s_log.Info("Precaching NPC Spawns...");
            npcRepository.GetNPCSpawns();

            s_log.Info("Precaching NPC Behaviours...");
            var npcBehaviours = npcRepository.GetNPCBehaviours();
            foreach (NPCModel npc in npcs)
            {
                npcRepository.GetNPCBehavioursByNPCID(npc.NPCID);
            }

            s_log.Info("Precaching NPC Behaviour Vars...");
            npcRepository.GetNPCBehaviourVars();
            foreach (NPCBehaviourModel npcBehaviour in npcBehaviours)
            {
                npcRepository.GetNPCBehaviourVarsByNPCBehaviourID(npcBehaviour.NPCBehaviourID);
            }

            s_log.Info("Precaching NPC Stats...");
            npcRepository.GetNPCStats();
            foreach (NPCModel npc in npcs)
            {
                npcRepository.GetNPCStatsByNPCID(npc.NPCID);
            }

            s_log.Info("Precaching abilities...");
            abilityRepository.GetAbilities();

            s_log.Info("Initialising serializer...");
            ProtocolUtility.InitialiseSerializer();

            s_log.Info("Creating world...");
            using (World world = new World(accountRepository, npcRepository, playerRepository, statsRepository, abilityRepository))
            {
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
}
