using Data.NPCs;
using Server.Utility;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Server.Zones
{
    public class Zone
    {
        private Fiber m_fiber = new Fiber();

        private ConcurrentDictionary<int, PlayerPeer> m_playersInZone = new ConcurrentDictionary<int, PlayerPeer>();

        private INPCRepository m_npcRepository;

        public IEnumerable<PlayerPeer> PlayersInZone { get; private set; }

        public List<NPCSpawnModel> m_npcSpawns;

        public int ID { get; private set; }

        public Zone(int zoneID, INPCRepository npcRepository)
        {
            m_npcRepository = npcRepository;

            m_npcSpawns = LoadZoneNPCSpawns();

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
    }
}
