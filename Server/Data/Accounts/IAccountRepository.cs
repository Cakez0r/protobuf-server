namespace Data.Accounts
{
    public interface IAccountRepository
    {
        AccountModel GetAccountByEmail(string email);
        AccountModel GetAccountByID(int accountID);
        AccountModel GetAccountByUsername(string username);
        AccountModel GetAccountByUsernameAndPasswordHash(string username, string passwordHash);
        AccountModel CreateAccount(string username, string passwordHash, string email);
        AccountModel UpdateAccount(int accountID, string passwordHash, string email);
    }
}
