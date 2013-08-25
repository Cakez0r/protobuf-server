using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Accounts
{
    public class MockAccountRepository : IAccountRepository
    {
        private Random m_rand = new Random(Environment.TickCount);

        public AccountModel GetAccountByEmail(string email)
        {
            return GetAccountModel(email);
        }

        public AccountModel GetAccountByID(int accountID)
        {
            return GetAccountModel("Account" + accountID);
        }

        public AccountModel GetAccountByUsername(string username)
        {
            return GetAccountModel(username);
        }

        public AccountModel GetAccountByUsernameAndPasswordHash(string username, string passwordHash)
        {
            return GetAccountModel(username);
        }

        public AccountModel CreateAccount(string username, string passwordHash, string email)
        {
            return GetAccountModel(username);
        }

        public AccountModel UpdateAccount(int accountID, string passwordHash, string email)
        {
            return GetAccountModel(email);
        }

        private AccountModel GetAccountModel(string name)
        {
            return new AccountModel()
            {
                AccountID = m_rand.Next(),
                DateCreated = DateTime.Now,
                Email = "fake@email.com",
                LastLoginDate = DateTime.Now,
                PasswordHash = "PasswordHash",
                Username = name
            };
        }
    }
}
