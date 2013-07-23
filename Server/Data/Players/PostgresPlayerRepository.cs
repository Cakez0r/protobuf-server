using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Players
{
    public class PostgresPlayerRepository : PostgresRepository, IPlayerRepository
    {
        public PlayerModel GetPlayerByID(int playerID)
        {
            return Function<PlayerModel>("GET_PlayerByID", new { _playerID = playerID }).SingleOrDefault();
        }

        public IEnumerable<PlayerModel> GetPlayersByAccountID(int accountID)
        {
            return Function<PlayerModel>("GET_PlayersByAccountID", new { _accountID = accountID });
        }

        public IEnumerable<PlayerStatModel> GetPlayerStatsByPlayerID(int playerID)
        {
            return Function<PlayerStatModel>("GET_PlayerStatsByPlayerID", new { _playerID = playerID });
        }

        public void UpdatePlayer(PlayerModel player)
        {
            Function("UPD_Player", player);
        }

        public void UpdatePlayerStat(PlayerStatModel playerStat)
        {
            Function("UPD_PlayerStat", playerStat);
        }
    }
}
