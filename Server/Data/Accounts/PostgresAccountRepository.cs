using System.Linq;

namespace Data.Accounts
{
    public class PostgresAccountRepository : PostgresRepository, IAccountRepository
    {
        public AccountModel GetAccountByEmail(string email)
        {
            return Function<AccountModel>("GET_AccountByEmail", new { _email = email }).SingleOrDefault();
        }

        public AccountModel GetAccountByID(int accountID)
        {
            return Function<AccountModel>("GET_AccountByID", new { _accountID = accountID }).SingleOrDefault();
        }

        public AccountModel GetAccountByUsername(string username)
        {
            return Function<AccountModel>("GET_AccountByUsername", new { _username = username }).SingleOrDefault();
        }

        public AccountModel GetAccountByUsernameAndPasswordHash(string username, string passwordHash)
        {
            return Function<AccountModel>("GET_AccountByUsernameAndPasswordHash", new { _username = username, _passwordHash = passwordHash }).SingleOrDefault();
        }

        public AccountModel CreateAccount(string username, string passwordHash, string email)
        {
            return Function<AccountModel>("INS_Account", new { _username = username, _passwordHash = passwordHash, _email = email }).SingleOrDefault();
        }

        public AccountModel UpdateAccount(int accountID, string passwordHash, string email)
        {
            return Function<AccountModel>("UPD_Account", new { _accountID = accountID, _passwordHash = passwordHash, _email = email }).SingleOrDefault();
        }
    }
}
