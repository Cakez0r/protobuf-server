using Data.Accounts;
using Data.Players;
using Protocol;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    public partial class PlayerPeer
    {
        private IAccountRepository m_accountRepository;
        private IPlayerRepository m_playerRepository;

        private PlayerModel m_player;

        private void Handle_AuthenticationAttempt(AuthenticationAttempt_C2S aa)
        {
            string hashedPassword = HashPassword(aa.Username, aa.Password);
            AccountModel account = m_accountRepository.GetAccountByUsernameAndPasswordHash(aa.Username, hashedPassword);

            AuthenticationAttempt_S2C.ResponseCode result;
            if (account != null)
            {
                PlayerModel player = m_playerRepository.GetPlayersByAccountID(account.AccountID).FirstOrDefault();

                if (player != null)
                {
                    m_player = player;
                    MaxHealth = 1000;
                    MaxPower = 1000;
                    Health = (int)(MaxHealth * m_player.Health);
                    Power = (int)(MaxPower * m_player.Power);

                    m_stats = m_playerRepository.GetPlayerStatsByPlayerID(player.PlayerID).ToDictionary(stat => stat.StatID, stat => stat.StatValue);

                    Introduction = new PlayerIntroduction() { PlayerID = ID, Name = player.Name };
                    Info("Logged in");
                    result = AuthenticationAttempt_S2C.ResponseCode.OK;

                    ChangeZone(0);

                    IsAuthenticated = true;
                }
                else
                {
                    result = AuthenticationAttempt_S2C.ResponseCode.Error;
                    Info("Username: {1} has no characters but tried to log in.", aa.Username);
                }
            }
            else
            {
                result = AuthenticationAttempt_S2C.ResponseCode.BadLogin;
                Info("Login failed with username: {1} and password: {2}", ID, aa.Username, aa.Password);
            }

            Respond(aa, new AuthenticationAttempt_S2C() { PlayerID = ID, Result = result });
        }

        private static string HashPassword(string username, string password)
        {
            using(SHA512 sha = new SHA512Managed())
            {
                string saltedPassword = string.Format("{0}{1}{2}", password, username.ToLower(), "{E54DC322-6F78-4500-86F2-8D9C688060B8}");
                byte[] input = Encoding.UTF8.GetBytes(password);
                byte[] output = sha.ComputeHash(input);
                return BitConverter.ToString(output).Replace("-", "");
            }
        }
    }
}
