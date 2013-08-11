using Data.Abilities;
using Data.Accounts;
using Data.NPCs;
using Data.Players;
using Data.Stats;
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
            var abilities = abilityRepository.GetAbilities();

            s_log.Info("Precaching ability behaviours...");
            var abilityBehaviours = abilityRepository.GetAbilityBehaviours();
            foreach (AbilityModel ability in abilities)
            {
                abilityRepository.GetAbilityBehavioursByAbilityID(ability.AbilityID);
            }

            s_log.Info("Precaching ability behaviour vars...");
            abilityRepository.GetAbilityBehaviourVars();
            foreach (AbilityBehaviourModel abilityBehaviour in abilityBehaviours)
            {
                abilityRepository.GetAbilityBehaviourVarsByAbilityBehaviourID(abilityBehaviour.AbilityBehaviourID);
            }

            s_log.Info("Creating world...");
            World world = new World(accountRepository, npcRepository, playerRepository, statsRepository);

            s_log.Info("Initialising serializer...");
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
