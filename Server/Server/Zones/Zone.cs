using Data.NPCs;
using NLog;
using Protocol;
using Server.NPC;
using Server.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Server.Zones
{
    public class Zone
    {
        private const float RELEVANCE_DISTANCE_SQR = 40 * 40;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private Fiber m_fiber = new Fiber();

        private ConcurrentDictionary<int, PlayerPeer> m_playersInZone = new ConcurrentDictionary<int, PlayerPeer>();

        private INPCRepository m_npcRepository;
        private NPCFactory m_npcFactory;

        public IEnumerable<PlayerPeer> PlayersInZone { get; private set; }

        private List<NPCSpawnModel> m_npcSpawns;

        private ReaderWriterLockSlim m_npcLock = new ReaderWriterLockSlim();
        private List<NPCInstance> m_npcs = new List<NPCInstance>();

        private DateTime m_lastUpdateTime = DateTime.Now;

        public int ID { get; private set; }

        public Zone(int zoneID, INPCRepository npcRepository, NPCFactory npcFactory)
        {
            ID = zoneID;

            m_npcRepository = npcRepository;
            m_npcFactory = npcFactory;

            m_npcSpawns = LoadZoneNPCSpawns();

            m_npcs = m_npcSpawns.Select(npcSpawn => npcFactory.SpawnNPC(npcSpawn)).ToList();

            PlayersInZone = Enumerable.Empty<PlayerPeer>();
        }

        private List<NPCSpawnModel> LoadZoneNPCSpawns()
        {
            IEnumerable<NPCSpawnModel> spawns = m_npcRepository.GetNPCSpawns();
            return spawns.Where(s => s.MapNumber == ID).ToList();
        }
        
        public void AddToZone(PlayerPeer player)
        {
            if (m_playersInZone.TryAdd(player.ID, player))
            {
                PlayersInZone = m_playersInZone.Values;
            }
        }

        public void RemoveFromZone(PlayerPeer player)
        {
            PlayerPeer removedPlayer = default(PlayerPeer);
            if (m_playersInZone.TryRemove(player.ID, out removedPlayer))
            {
                PlayersInZone = m_playersInZone.Values;
            }
        }

        public void Update()
        {
            m_fiber.Enqueue(InternalUpdate);
        }

        private void InternalUpdate()
        {
            TimeSpan dt = DateTime.Now - m_lastUpdateTime;
            
            m_npcLock.EnterWriteLock();
            foreach (NPCInstance npc in m_npcs)
            {
                npc.Update(dt);
            }
            m_npcLock.ExitWriteLock();

            m_lastUpdateTime = DateTime.Now;
        }

        public void GatherNPCStatesForPlayer(PlayerPeer player, List<NPCStateUpdate> playerNPCStates)
        {
            playerNPCStates.Clear();
            Vector2 playerPosition = new Vector2(player.LatestStateUpdate.X, player.LatestStateUpdate.Y);
            m_npcLock.EnterReadLock();
            foreach (NPCInstance npc in m_npcs)
            {
                Vector2 npcPosition = new Vector2((float)npc.NPCSpawnModel.X, (float)npc.NPCSpawnModel.Y);
                float distanceSqr = (playerPosition - npcPosition).LengthSquared();
                if (distanceSqr <= RELEVANCE_DISTANCE_SQR)
                {
                    playerNPCStates.Add(npc.StateUpdate);
                }
            }
            m_npcLock.ExitReadLock();
        }
    }
}
