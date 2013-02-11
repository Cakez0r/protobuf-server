using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data
{
    public static class MySQL
    {
        public static bool TestConnection(out Exception exception)
        {
            exception = null;

            try
            {
                using (IDbConnection db = new MySqlConnection(Settings.ConnectionString))
                {
                    db.Open();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }

            return true;
        }

        public static IEnumerable<T> StoredProcedure<T>(string name, object parameters)
        {
            using (IDbConnection db = new MySqlConnection(Settings.ConnectionString))
            {
                db.Open();
                return db.Query<T>(name, parameters, null, true, null, CommandType.StoredProcedure);
            }
        }
    }
}
