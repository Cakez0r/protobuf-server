using Data.NPCs;
using Protocol;
using Server.NPC;
using Server.Utility;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Server.Zones
{
    public class Zone
    
        private Fiber m_fiber = new Fiber();

        private ConcurrentDictionary<int, PlayerPeer> m_playersInZone = new ConcurrentDictionary<int, PlayerPeer>();

        private INPCRepository m_npcRepository;

        public IEnumerable<PlayerPeer> PlayersInZone { get; private set; }

        private List<NPCSpawnModel> m_npcSpawns;

        private List<NPCInstance> m_npcs = new List<NPCInstance>();

        public int ID { get; private set; }

        public Zone(int zoneID, INPCRepository npcRepository)
        {
            m_npcRepository = npcRepository;

            m_npcSpawns = LoadZoneNPCSpawns();

            //For now, NPCs will just be static spawns that don't move...
            m_npcs = m_npcSpawns.Select(npc => new NPCInstance() { NPCModel = m_npcRepository.GetNPCByID(npc.NPCID) }).ToList();

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
        }

        public void GatherNPCStatesForPlayer(PlayerPeer player, List<NPCStateUpdate> playerNPCStates)
        {
            playerNPCStates.Clear();
            Vector2 playerPosition = new Vector2(player.LatestStateUpdate.X, player.LatestStateUpdate.Y);
            foreach (NPCInstance npc in m_npcs)
            {
            }
        }
    }
}
