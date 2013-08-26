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

        private INPCRepository m_npcRepository;
        private NPCFactory m_npcFactory;

        public ReadOnlyCollection<PlayerPeer> PlayersInZone { get; private set; }

        private List<NPCSpawnModel> m_npcSpawns;

        private ReaderWriterLockSlim m_npcLock = new ReaderWriterLockSlim();
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

            PlayersInZone = Enumerable.Empty<PlayerPeer>().ToList().AsReadOnly();

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
                PlayersInZone = (ReadOnlyCollection<PlayerPeer>)m_playersInZone.Values;
            }
        }

        public void RemoveFromZone(PlayerPeer player)
        {
            PlayerPeer removedPlayer = default(PlayerPeer);
            if (m_playersInZone.TryRemove(player.ID, out removedPlayer))
            {
                PlayersInZone = (ReadOnlyCollection<PlayerPeer>)m_playersInZone.Values;
            }
        }

        private void Update()
        {
            m_zoneUpdateTimer.Restart();
            TimeSpan dt = DateTime.Now - m_lastUpdateTime;
            
            m_npcLock.EnterWriteLock();
            foreach (NPCInstance npc in m_npcs.Values)
            {
                npc.Update(dt);
            }
            m_npcLock.ExitWriteLock();

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

        public void GatherNPCStatesForPlayer(PlayerPeer player, List<NPCStateUpdate> playerNPCStates)
        {
            Vector2 playerPosition = player.Position;
            m_npcLock.EnterReadLock();
            foreach (NPCInstance npc in m_npcs.Values)
            {
                if (npc.IsDead)
                {
                    continue;
                }

                if (Vector2.DistanceSquared(playerPosition, npc.Position) <= RELEVANCE_DISTANCE_SQR)
                {
                    playerNPCStates.Add(npc.StateUpdate);
                }
            }
            m_npcLock.ExitReadLock();
        }

        public void AbilityUsed(AbilityInstance ability)
        {

        }

        public Task<ITargetable> GetTarget(int id)
        {
            return m_fiber.Enqueue(() =>
            {
                PlayerPeer player = default(PlayerPeer);
                NPCInstance npc = default(NPCInstance);
                ITargetable target = default(ITargetable);

                if (m_playersInZone.TryGetValue(id, out player))
                {
                    target = (ITargetable)player;
                }
                else if (m_npcs.TryGetValue(id, out npc))
                {
                    target = (ITargetable)npc;
                }

                return target;
            }, false);
        }

        public void SendMessageToZone(string sender, string message)
        {
            ChatMessage cm = new ChatMessage() { SenderName = sender, Message = message };
            foreach (PlayerPeer player in PlayersInZone)
            {
                player.Send(cm);
            }
        }
    }
}
