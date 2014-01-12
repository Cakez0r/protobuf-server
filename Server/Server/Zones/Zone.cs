using Data.NPCs;
using NLog;
using Protocol;
using Server.Map;
using Server.NPC;
using Server.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Zones
{
    public class Zone
    {
        private static Logger s_log = LogManager.GetCurrentClassLogger();

        private const int TARGET_UPDATE_TIME_MS = 50;
        private const float RELEVANCE_DISTANCE_SQR = 4000 * 4000;

        private DateTime m_lastUpdateTime = DateTime.Now;
        private Stopwatch m_zoneUpdateTimer = new Stopwatch();
        private Fiber m_fiber = new Fiber();

        private MapData m_mapData;

        private ConcurrentDictionary<int, PlayerPeer> m_playersInZone = new ConcurrentDictionary<int, PlayerPeer>();
        private PointKDTree<IEntity> m_playerTree = new PointKDTree<IEntity>();
        private PlayerPeer[] m_playerArray;
        private bool m_playerListIsDirty;

        private INPCRepository m_npcRepository;
        private NPCFactory m_npcFactory;
        private List<NPCSpawnModel> m_npcSpawns;
        private Dictionary<int, NPCInstance> m_npcs = new Dictionary<int, NPCInstance>();
        private PointKDTree<IEntity> m_npcTree = new PointKDTree<IEntity>();
        private NPCInstance[] m_npcArray;

        public long LastUpdateLength { get; private set; }
        public int ID { get; private set; }

        public Zone(int zoneID, INPCRepository npcRepository, NPCFactory npcFactory, MapData mapData)
        {
            ID = zoneID;

            m_npcRepository = npcRepository;
            m_npcFactory = npcFactory;

            m_mapData = mapData;

            m_npcSpawns = LoadZoneNPCSpawns();
            m_npcArray = new NPCInstance[m_npcSpawns.Count];
            for (int i = 0; i < m_npcSpawns.Count; i++)
            {
                NPCInstance npcInstance = npcFactory.CreateNPC(m_fiber, m_npcSpawns[i], mapData);
                m_npcs.Add(npcInstance.ID, npcInstance);
                m_npcArray[i] = npcInstance;
            }

            m_playerArray = Enumerable.Empty<PlayerPeer>().ToArray();

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
                lock (m_playerArray)
                {
                    m_playerListIsDirty = true;
                }
            }
        }

        public void RemoveFromZone(PlayerPeer player)
        {
            PlayerPeer removedPlayer = default(PlayerPeer);
            if (m_playersInZone.TryRemove(player.ID, out removedPlayer))
            {
                lock (m_playerArray)
                {
                    m_playerListIsDirty = true;
                }
            }
        }

        public List<IEntity> GatherEntitiesInRange(BoundingBox range)
        {
            List<IEntity> result = new List<IEntity>();

            m_playerTree.GatherRange(range, result);
            m_npcTree.GatherRange(range, result);

            return result;
        }

        private void Update()
        {
            m_zoneUpdateTimer.Restart();
            TimeSpan dt = DateTime.Now - m_lastUpdateTime;
            
            foreach (var kvp in m_npcs)
            {
                kvp.Value.Update(dt);
            }

            lock (m_playerArray)
            {
                if (m_playerListIsDirty)
                {
                    m_playerArray = m_playersInZone.Values.ToArray();
                    m_playerListIsDirty = false;
                }
            }

            m_playerTree.Build(m_playerArray);

            m_npcTree.Build(m_npcArray);

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
            m_fiber.Enqueue(() =>
            {
                ChatMessage cm = new ChatMessage() { SenderName = sender, Message = message };
                for (int i = 0; i < m_playerArray.Length; i++)
                {
                    m_playerArray[i].Send(cm);
                }
            }, false);
        }
    }
}
