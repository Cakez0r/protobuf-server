using Data.NPCs;
using NLog;
using Protocol;
using Server.Abilities;
using Server.NPC;
using Server.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Zones
{
    public class Zone
    {
        private const int TARGET_UPDATE_TIME_MS = 50;
        private const float RELEVANCE_DISTANCE_SQR = 40 * 40;

        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private Fiber m_fiber = new Fiber();

        private ConcurrentDictionary<int, PlayerPeer> m_playersInZone = new ConcurrentDictionary<int, PlayerPeer>();
        private volatile IEntity[] m_playersArray = new IEntity[0];
        private volatile Link[] m_links = new Link[0];
        private volatile BoundingBox[] m_boundaries = new BoundingBox[0];
        private volatile ReaderWriterLockSlim m_buildLock = new ReaderWriterLockSlim();

        private INPCRepository m_npcRepository;
        private NPCFactory m_npcFactory;

        private List<NPCSpawnModel> m_npcSpawns;

        private Dictionary<int, NPCInstance> m_npcs = new Dictionary<int, NPCInstance>();

        private DateTime m_lastUpdateTime = DateTime.Now;

        private Stopwatch m_zoneUpdateTimer = new Stopwatch();
        public long LastUpdateLength { get; private set; }

        public int ID { get; private set; }

        public Zone(int zoneID, INPCRepository npcRepository, NPCFactory npcFactory)
        {
            ID = zoneID;

            m_npcRepository = npcRepository;
            m_npcFactory = npcFactory;

            m_npcSpawns = LoadZoneNPCSpawns();

            foreach (NPCSpawnModel spawn in m_npcSpawns)
            {
                NPCInstance npcInstance = npcFactory.SpawnNPC(m_fiber, spawn);
                m_npcs.Add(npcInstance.ID, npcInstance);
            }

            m_playersArray = Enumerable.Empty<IEntity>().ToArray();
            m_boundaries = Enumerable.Range(0, 5000).Select(i => new BoundingBox()).ToArray();
            m_links = Enumerable.Range(0, 5000).Select(i => new Link()).ToArray();

            m_fiber.Enqueue(Update);
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
                m_buildLock.EnterWriteLock();
                m_playersArray = m_playersInZone.Values.ToArray();
                root = KDTree.KDSort(m_playersArray, m_links, m_boundaries, 0, m_playersArray.Length - 1, false);
                m_buildLock.ExitWriteLock();
            }
        }

        public void RemoveFromZone(PlayerPeer player)
        {
            PlayerPeer removedPlayer = default(PlayerPeer);
            if (m_playersInZone.TryRemove(player.ID, out removedPlayer))
            {
                m_buildLock.EnterWriteLock();
                m_playersArray = m_playersInZone.Values.ToArray();
                m_buildLock.ExitWriteLock();
            }
        }

        public void GatherEntities(BoundingBox range, List<IEntity> result)
        {
            m_buildLock.EnterReadLock();
            KDTree.Query(m_playersArray, m_links, m_boundaries, ref range, root, result);
            m_buildLock.ExitReadLock();
        }

        int root = 0;
        private void Update()
        {
            m_zoneUpdateTimer.Restart();
            TimeSpan dt = DateTime.Now - m_lastUpdateTime;
            
            foreach (var kvp in m_npcs)
            {
                kvp.Value.Update(dt);
            }

            if (m_playersArray.Length > 0)
            {
                //Stopwatch sw = Stopwatch.StartNew();
                KDTree.LeftCount = 0;
                KDTree.RightCount = 0;
                if (m_buildLock.TryEnterWriteLock(30))
                {
                    root = KDTree.KDSort(m_playersArray, m_links, m_boundaries, 0, m_playersArray.Length - 1, false);
                    KDTree.RightCount++;
                    KDTree.LeftCount++;
                    s_log.Trace("Balance: {0}", KDTree.LeftCount > KDTree.RightCount ? (float)KDTree.LeftCount / KDTree.RightCount : (float)KDTree.RightCount / KDTree.LeftCount);
                    m_buildLock.ExitWriteLock();
                }
                else
                {
                    s_log.Warn("Rebuild skipped!");
                }
                //sw.Stop();
                //if (sw.ElapsedMilliseconds > 10)
                //{
                //    s_log.Warn("Rebuild time was {0}ms", sw.ElapsedMilliseconds);
                //}
            }

            m_lastUpdateTime = DateTime.Now;
            m_zoneUpdateTimer.Stop();
            LastUpdateLength = m_zoneUpdateTimer.ElapsedMilliseconds;

            int restTime = TARGET_UPDATE_TIME_MS - (int)LastUpdateLength;

            if (restTime >= 0)
            {
                m_fiber.Schedule(Update, TimeSpan.FromMilliseconds(restTime));
            }
            else
            {
                s_log.Warn("Zone {0} update ran into overtime by {1}ms", ID, Math.Abs(restTime));
                m_fiber.Enqueue(Update);
            }
        }

        public void GatherNPCStatesForPlayer(PlayerPeer player, EntityStateUpdate[] entities, ref int bufferCount)
        {
            Vector2 playerPosition = player.Position;
            foreach (var kvp in m_npcs)
            {
                NPCInstance npc = kvp.Value;
                if (npc.IsDead)
                {
                    continue;
                }

                if (Vector2.DistanceSquared(playerPosition, npc.Position) <= RELEVANCE_DISTANCE_SQR)
                {
                    entities[bufferCount++] = npc.GetStateUpdate();
                }
            }
        }

        public void AbilityUsed(AbilityInstance ability)
        {

        }

        public Task<IEntity> GetTarget(int id)
        {
            return m_fiber.Enqueue(() =>
            {
                PlayerPeer player = default(PlayerPeer);
                NPCInstance npc = default(NPCInstance);
                IEntity target = default(IEntity);

                if (m_playersInZone.TryGetValue(id, out player))
                {
                    target = (IEntity)player;
                }
                else if (m_npcs.TryGetValue(id, out npc))
                {
                    target = (IEntity)npc;
                }

                return target;
            }, false);
        }

        public void SendMessageToZone(string sender, string message)
        {
            //ChatMessage cm = new ChatMessage() { SenderName = sender, Message = message };
            //foreach (PlayerPeer player in PlayersInZone)
            //{
            //    player.Send(cm);
            //}
        }
    }
}
