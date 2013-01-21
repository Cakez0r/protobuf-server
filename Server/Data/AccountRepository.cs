using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using MySql.Data.MySqlClient;

namespace Data
{
    public class AccountRepository
    {
        public AccountModel GetWithLogin(string name, string password)
        {
            return MySQL.StoredProcedure<AccountModel>("get_account_with_login", new { Name = name, Password = password }).FirstOrDefault();
        }
    }
}
