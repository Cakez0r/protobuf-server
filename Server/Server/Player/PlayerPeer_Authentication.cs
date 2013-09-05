using Data.Accounts;
using Data.Players;
using Protocol;
using Server.Gameplay;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private static ConcurrentDictionary<int, bool> s_loggedInAccounts = new ConcurrentDictionary<int, bool>();
        public static ConcurrentDictionary<int, bool> LoggedInAccounts 
        {
            get { return s_loggedInAccounts; }
        }

        public int AccountID { get; private set; }

        private void Handle_AuthenticationAttempt(AuthenticationAttempt_C2S aa)
        {
            string hashedPassword = HashPassword(aa.Username, aa.Password);
            AccountModel account = m_accountRepository.GetAccountByUsernameAndPasswordHash(aa.Username, hashedPassword);

            AuthenticationAttempt_S2C response = new AuthenticationAttempt_S2C() { PlayerID = ID };
            if (account != null)
            {
                bool alreadyOnline = false;
                if (s_loggedInAccounts.TryGetValue(account.AccountID, out alreadyOnline) && alreadyOnline)
                {
                    Info("Username: {0} is already online but tried to log in", aa.Username);
                    response.Result = AuthenticationAttempt_S2C.ResponseCode.AlreadyLoggedIn;
                }
                else
                {
                    response.Result = LoadPlayer(account);

                    if (response.Result == AuthenticationAttempt_S2C.ResponseCode.OK)
                    {
                        AccountID = account.AccountID;
                        s_loggedInAccounts[account.AccountID] = true;
                        response.X = (float)m_player.X;
                        response.Y = (float)m_player.Y;
                        response.ZoneID = m_player.Map;

                        response.Stats = new Dictionary<int, float>();

                        foreach (var kvp in m_stats)
                        {
                            response.Stats.Add((int)kvp.Key, kvp.Value.StatValue);
                        }

                        ChangeZone(response.ZoneID);

                        IsAuthenticated = true;
                    }
                }
            }
            else
            {
                response.Result = AuthenticationAttempt_S2C.ResponseCode.BadLogin;
                Info("Login failed with username: {1} and password: {2}", ID, aa.Username, aa.Password);
            }

            Respond(aa, response);
        }

        private AuthenticationAttempt_S2C.ResponseCode LoadPlayer(AccountModel account)
        {
            AuthenticationAttempt_S2C.ResponseCode result;
            PlayerModel player = m_playerRepository.GetPlayersByAccountID(account.AccountID).FirstOrDefault();

            if (player != null)
            {
                m_player = player;
                m_stats = m_playerRepository.GetPlayerStatsByPlayerID(player.PlayerID).ToDictionary(stat => (StatType)stat.StatID, stat => stat);

                MaxHealth = (ushort)Formulas.StaminaToHealth(GetStatValue(StatType.Stamina));
                Level = Formulas.XPToLevel(GetStatValue(StatType.XP));
                MaxPower = (ushort)Formulas.LevelToPower(Level);
                Health = (ushort)(MaxHealth * m_player.Health);
                Power = (ushort)(MaxPower * m_player.Power);
                Name = account.Username;

                RecreateIntroduction();

                Info("Player loaded for account {0}", account.Username);
                result = AuthenticationAttempt_S2C.ResponseCode.OK;
            }
            else
            {
                result = AuthenticationAttempt_S2C.ResponseCode.Error;
                Info("Username: {1} has no characters but tried to log in.", account.Username);
            }
            return result;
        }

        private void RecreateIntroduction()
        {
            m_introduction = new EntityIntroduction()
            {
                ID = ID,
                Name = Name,
                Level = Level,
                MaxHealth = MaxHealth,
                MaxPower = MaxPower,
                ModelID = 0
            };
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
