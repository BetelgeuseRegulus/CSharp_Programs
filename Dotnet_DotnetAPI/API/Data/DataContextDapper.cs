using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;

namespace API.Data
{
    class DataContextDapper
    {
        private readonly IConfiguration _config;

        public DataContextDapper(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(
                _config.GetConnectionString("DefaultConnection")
            );

            return dbConnection.Query<T>(sql);
        }

        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(
                _config.GetConnectionString("DefaultConnection")
            );
            return dbConnection.QuerySingle<T>(sql);
        }

        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(
                _config.GetConnectionString("DefaultConnection")
            );
            return dbConnection.Execute(sql) > 0;
        }

        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(
                _config.GetConnectionString("DefaultConnection")
            );

            return dbConnection.Execute(sql);
        }

        public bool ExecuteWithParameter(string sql, List<SqlParameter> parameters)
        {
            SqlCommand commandWithParameters = new SqlCommand(sql);

            foreach (SqlParameter sqlParameter in parameters)
            {
                commandWithParameters.Parameters.Add(sqlParameter);
            }

            SqlConnection dbConnection = new SqlConnection(
                _config.GetConnectionString("DefaultConnection")
            );

            dbConnection.Open();

            commandWithParameters.Connection = dbConnection;
            int rowsAffected = commandWithParameters.ExecuteNonQuery();

            dbConnection.Close();

            return rowsAffected > 0;
        }

        public bool ExecuteSqlWithParameters(string sql, DynamicParameters parameters)
        {
            IDbConnection dbConnection = new SqlConnection(
                _config.GetConnectionString("DefaultConnection")
            );
            return dbConnection.Execute(sql, parameters) > 0;
        }

        public IEnumerable<T> LoadDataWithParameters<T>(string sql, DynamicParameters parameters)
        {
            IDbConnection dbConnection = new SqlConnection(
                _config.GetConnectionString("DefaultConnection")
            );
            return dbConnection.Query<T>(sql, parameters);
        }

        public T LoadDataSingleWithParameters<T>(string sql, DynamicParameters parameters)
        {
            IDbConnection dbConnection = new SqlConnection(
                _config.GetConnectionString("DafaultConnection")
            );
            return dbConnection.QuerySingle<T>(sql, parameters);
        }
    }
}
