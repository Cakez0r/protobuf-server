using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Players
{
    public interface IPlayerRepository
    {
        PlayerModel GetPlayerByID(int playerID);
        IEnumerable<PlayerModel> GetPlayersByAccountID(int accountID);
        IEnumerable<PlayerStatModel> GetPlayerStatsByPlayerID(int playerID);
        void UpdatePlayer(int playerID, int accountID, string name, float health, float power, long money, int map, float x, float y, float rotation);
        void UpdatePlayerStat(int playerStatID, int playerID, int statID, float statValue);
    }
}
