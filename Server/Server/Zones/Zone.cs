using Data.NPCs;
using Protocol;
using Server.NPC;
using Server.Utility;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Server.Zones
{
    public class Zone
    {
        private const float RELEVANCE_DISTANCE_SQR = 40 * 40;

        private Fiber m_fiber = new Fiber();

        private ConcurrentDictionary<int, PlayerPeer> m_playersInZone = new ConcurrentDictionary<int, PlayerPeer>();

        private INPCRepository m_npcRepository;

        public IEnumerable<PlayerPeer> PlayersInZone { get; private set; }

        private List<NPCSpawnModel> m_npcSpawns;

        private ReaderWriterLockSlim m_npcLock = new ReaderWriterLockSlim();
        private List<NPCInstance> m_npcs = new List<NPCInstance>();

        public int ID { get; private set; }

        public Zone(int zoneID, INPCRepository npcRepository)
        {
            m_npcRepository = npcRepository;

            m_npcSpawns = LoadZoneNPCSpawns();

            //For now, NPCs will just be static spawns that don't move...
            m_npcs = m_npcSpawns.Select(npcSpawn => new NPCInstance(m_npcRepository.GetNPCByID(npcSpawn.NPCID), npcSpawn)).ToList();

            PlayersInZone = Enumerable.Empty<PlayerPeer>();
            ID = zoneID;
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
            m_npcLock.EnterWriteLock();
            //TODO: Patrollin'
            foreach (NPCInstance npc in m_npcs)
            {
                npc.StateUpdate.X = (float)npc.NPCSpawnModel.X;
                npc.StateUpdate.Y = (float)npc.NPCSpawnModel.Y;
                npc.StateUpdate.Rotation = npc.NPCSpawnModel.Rotation;
            }
            m_npcLock.ExitWriteLock();
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
