using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Configuration;
using System.Data;

namespace Data
{
    public abstract class PostgresRepository
    {
        private string m_connectionString;

        public PostgresRepository()
        {
            m_connectionString = ConfigurationManager.AppSettings["ConnectionString"];
        }

        protected IEnumerable<T> Function<T>(string functionName, object parameters)
        {
            using (NpgsqlConnection sql = new NpgsqlConnection(m_connectionString))
            {
                sql.Open();
                return sql.Query<T>(functionName, parameters, null, true, null, CommandType.StoredProcedure);
            }
        }

        protected int Function(string functionName, object parameters)
        {
            using (NpgsqlConnection sql = new NpgsqlConnection(m_connectionString))
            {
                sql.Open();
                return sql.Execute(functionName, parameters, null, null, CommandType.StoredProcedure);
            }
        }
    }
}
