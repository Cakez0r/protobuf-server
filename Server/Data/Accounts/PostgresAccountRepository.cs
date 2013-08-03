using System.Linq;

namespace Data.Accounts
{
    public class PostgresAccountRepository : PostgresRepository, IAccountRepository
    {
        public AccountModel GetAccountByEmail(string email)
        {
            return Function<AccountModel>("GET_AccountByEmail(@_email)", new { _email = email }).SingleOrDefault();
        }

        public AccountModel GetAccountByID(int accountID)
        {
            return Function<AccountModel>("GET_AccountByID(@_accountID)", new { _accountID = accountID }).SingleOrDefault();
        }

        public AccountModel GetAccountByUsername(string username)
        {
            return Function<AccountModel>("GET_AccountByUsername(@_username)", new { _username = username }).SingleOrDefault();
        }

        public AccountModel GetAccountByUsernameAndPasswordHash(string username, string passwordHash)
        {
            return Function<AccountModel>("GET_AccountByUsernameAndPasswordHash(@_username, @_passwordHash)", new { _username = username, _passwordHash = passwordHash }).SingleOrDefault();
        }

        public AccountModel CreateAccount(string username, string passwordHash, string email)
        {
            return Function<AccountModel>("INS_Account(@_username, @_passwordHash, @_email)", new { _username = username, _passwordHash = passwordHash, _email = email }).SingleOrDefault();
        }

        public AccountModel CreateAccount2(string username, string passwordHash, string email)
        {
            return Function<AccountModel>("INS_Account", new { _username = username, _passwordHash = passwordHash, _email = email }).SingleOrDefault();
        }

        public AccountModel UpdateAccount(int accountID, string passwordHash, string email)
        {
            return Function<AccountModel>("UPD_Account(@_accountID, @_passwordHash, @_email)", new { _accountID = accountID, _passwordHash = passwordHash, _email = email }).SingleOrDefault();
        }
    }
}
