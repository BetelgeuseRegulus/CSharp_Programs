using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Data
{
    class DataContextDapper
    {
        private readonly IConfiguration _config;

        public DataContextDapper(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// 根据传入的 SQL 查询语句，从数据库中获取多个 T 类型的数据。
        /// </summary>
        /// <typeparam name="T">数据模型类型</typeparam>
        /// <param name="sql">要执行的 SQL 查询语句</param>
        /// <returns>返回IEnumerable T，可枚举可遍历的 T 类型对象集合</returns>
        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }

        /// <summary>
        /// 根据传入的 SQL 查询语句，从数据库中获取单条 T 类型的数据。
        /// </summary>
        /// <typeparam name="T">数据模型类型</typeparam>
        /// <param name="sql">要执行的 SQL 查询语句</param>
        /// <returns>返回一个 T 类型的对象，如果查询不到或返回多条会抛出异常</returns>
        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool ExecuteSqlWithParameter(string sql, List<SqlParameter> parameters)
        {
            SqlCommand commandWithParams = new SqlCommand(sql);

            foreach (SqlParameter parameter in parameters)
            {
                commandWithParams.Parameters.Add(parameter);
            }

            SqlConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            dbConnection.Open();

            commandWithParams.Connection = dbConnection;

            int rowsAffected = commandWithParams.ExecuteNonQuery();
            dbConnection.Close();

            return rowsAffected > 0;
        }
    }
}