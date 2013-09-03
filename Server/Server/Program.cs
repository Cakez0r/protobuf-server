using Data.Abilities;
using Data.Accounts;
using Data.NPCs;
using Data.Players;
using Data.Stats;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using NLog;
using Protocol;
using Server.Utility;
using Server.Zones;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Program
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        class FakeEntity : IEntity
        {
            public int ID
            {
                get { throw new System.NotImplementedException(); }
            }

            public string Name
            {
                get { throw new System.NotImplementedException(); }
            }

            public Utility.Vector2 Position { get; set; }

            public byte Level
            {
                get { throw new System.NotImplementedException(); }
            }

            public int Health
            {
                get { throw new System.NotImplementedException(); }
            }

            public int Power
            {
                get { throw new System.NotImplementedException(); }
            }

            public int MaxHealth
            {
                get { throw new System.NotImplementedException(); }
            }

            public int MaxPower
            {
                get { throw new System.NotImplementedException(); }
            }

            public bool IsDead
            {
                get { throw new System.NotImplementedException(); }
            }

            public void ApplyHealthDelta(int delta, IEntity source)
            {
                throw new System.NotImplementedException();
            }

            public void ApplyPowerDelta(int delta, IEntity source)
            {
                throw new System.NotImplementedException();
            }

            public void ApplyXPDelta(int delta, IEntity source)
            {
                throw new System.NotImplementedException();
            }

            public EntityStateUpdate GetStateUpdate()
            {
                throw new System.NotImplementedException();
            }
        }

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
