using System;

namespace Data.Accounts
{
    public class AccountModel
    {
        public int AccountID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastLoginDate { get; set; }
    }
}
