using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Models;
using Dapper;

namespace API.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;

        public ReusableSql(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        public bool UpsertUser(UserComplete user)
        {
            string sql =
                @"EXEC TutorialAppSchema.spUser_Upsert
                @FirstName = @FirstNameParameter,
                @LastName = @LastNameParameter,
                @Email = @EmailParameter,
                @Gender = @GenderParameter,
                @Active = @ActiveParameter,
                @JobTitle = @JobTitleParameter,
                @Department = @DepartmentParameter,
                @Salary = @SalaryParameter,
                @UserId = @UserIdParameter,
            ";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@Firstname", user.FirstName, DbType.String);
            sqlParameters.Add("@LastName", user.LastName, DbType.String);
            sqlParameters.Add("@Email", user.Email, DbType.String);
            sqlParameters.Add("@Gender", user.Gender, DbType.String);
            sqlParameters.Add("@Active", user.Active, DbType.Boolean);
            sqlParameters.Add("@JobTitle", user.JobTitle, DbType.String);
            sqlParameters.Add("@Department", user.Department, DbType.String);
            sqlParameters.Add("@Salary", user.Salary, DbType.Decimal);
            sqlParameters.Add("@UserId", user.UserId, DbType.Int32);

            return _dapper.ExecuteSqlWithParameters(sql, sqlParameters);
        }
    }
}
