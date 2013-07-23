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
        void UpdatePlayer(PlayerModel player);
        void UpdatePlayerStat(PlayerStatModel playerStat);
    }
}
