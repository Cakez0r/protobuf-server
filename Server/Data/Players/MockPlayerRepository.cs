using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Data.Players
{
    public class MockPlayerRepository : IPlayerRepository
    {
        private Random m_rand = new Random(Environment.TickCount);
        private static int s_nextPlayerID;

        public PlayerModel GetPlayerByID(int playerID)
        {
            return GetPlayerModel(playerID.ToString());
        }

        public IEnumerable<PlayerModel> GetPlayersByAccountID(int accountID)
        {
            return Enumerable.Range(0, 1).Select(i => GetPlayerModel(accountID.ToString()));
        }

        public IEnumerable<PlayerStatModel> GetPlayerStatsByPlayerID(int playerID)
        {
            return GetPlayerStats();
        }

        public void UpdatePlayer(int playerID, int accountID, string name, float health, float power, long money, int map, float x, float y, float rotation)
        {
        }

        public void UpdatePlayerStat(int playerStatID, int playerID, int statID, float statValue)
        {
        }

        private PlayerModel GetPlayerModel(string name)
        {
            int playerID = Interlocked.Increment(ref s_nextPlayerID);

            PlayerModel player = new PlayerModel()
            {
                AccountID = m_rand.Next(),
                Health = 1,
                Map = 0,
                Money = 0,
                Name = name,
                PlayerID = m_rand.Next(),
                Power = 1,
                Rotation = 0,
                X = m_rand.Next(0, 6000),
                Y = m_rand.Next(0, 6000)
            };
            player.X = 140;
            player.Y = 140;

            return player;
        }

        private IEnumerable<PlayerStatModel> GetPlayerStats()
        {
            return new List<PlayerStatModel>()
            {
                new PlayerStatModel() { StatID = 1, StatValue = 250 },
                new PlayerStatModel() { StatID = 2, StatValue = 5000000 }
            };
        }
    }
}
