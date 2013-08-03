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
            return Function<PlayerModel>("GET_PlayerByID(@_playerID)", new { _playerID = playerID }).SingleOrDefault();
        }

        public IEnumerable<PlayerModel> GetPlayersByAccountID(int accountID)
        {
            return Function<PlayerModel>("GET_PlayersByAccountID(@_accountID)", new { _accountID = accountID });
        }

        public IEnumerable<PlayerStatModel> GetPlayerStatsByPlayerID(int playerID)
        {
            return Function<PlayerStatModel>("GET_PlayerStatsByPlayerID(@_playerID)", new { _playerID = playerID });
        }

        public void UpdatePlayer(int playerID, int accountID, string name, float health, float power, long money, int map, float x, float y, float rotation)
        {
            Function("UPD_Player(@_playerID, @_accountID, @_name, @_health, @_power, @_money, @_map, @_position, @_rotation)",
                new 
                { 
                    _playerID = playerID, 
                    _accountID = accountID, 
                    _name = name, 
                    _health = health, 
                    _power = power, 
                    _money = money, 
                    _map = map, 
                    _position = new NpgsqlTypes.NpgsqlPoint(x, y), 
                    _rotation = rotation 
                });
        }

        public void UpdatePlayerStat(int playerStatID, int playerID, int statID, float statValue)
        {
            Function("UPD_PlayerStat(@_playerStatID, @_playerID, @_statID, @_statValue)",
                new
                {
                    _playerStatID = playerStatID,
                    _playerID = playerID,
                    _statID = statID,
                    _statValue = statValue
                });
        }
    }
}
