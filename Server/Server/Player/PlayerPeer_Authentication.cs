using Data.Accounts;
using Protocol;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    public partial class PlayerPeer
    {
        private IAccountRepository m_accountRepository;

        private void Handle_AuthenticationAttempt(AuthenticationAttempt_C2S aa)
        {
            string hashedPassword = HashPassword(aa.Username, aa.Password);
            AccountModel account = m_accountRepository.GetAccountByUsernameAndPasswordHash(aa.Username, hashedPassword);

            AuthenticationAttempt_S2C.ResponseCode result;
            if (account != null)
            {
                Introduction = new PlayerIntroduction() { PlayerID = ID, Name = "[" + ID.ToString() + "]" + account.Username };
                s_log.Info("[{0}] Authenticated as {1}", ID, account.Username);
                result = AuthenticationAttempt_S2C.ResponseCode.OK;
                IsAuthenticated = true;

                ChangeZone(0);
            }
            else
            {
                result = AuthenticationAttempt_S2C.ResponseCode.BadLogin;
                s_log.Info("[{0}] Login failed with username: {1} and password: {2}", ID, aa.Username, aa.Password);
            }

            Respond(aa, new AuthenticationAttempt_S2C() { PlayerID = ID, Result = result });
        }

        private static string HashPassword(string username, string password)
        {
            using(SHA512 sha = new SHA512Managed())
            {
                string saltedPassword = string.Format("{0}{1}{2}", password, username, "{E54DC322-6F78-4500-86F2-8D9C688060B8}");
                byte[] input = Encoding.UTF8.GetBytes(password);
                byte[] output = sha.ComputeHash(input);
                return BitConverter.ToString(output).Replace("-", "");
            }
        }
    }
}
