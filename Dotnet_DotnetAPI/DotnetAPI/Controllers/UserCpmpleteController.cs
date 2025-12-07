using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controller;

// [] 表示特性(Attribute),给当前类(方法,属性)添加"元数据"(metadata), 启用特性的额外行为.
[Authorize]
[ApiController] // 开启 Web API 的智能行为：自动模型验证、自动 400 返回、改进参数绑定
[Route("[Controller]")]
public class UserCpmpleteController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly ReusableSql _reusableSql;

    public UserCpmpleteController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _reusableSql = new ReusableSql(config);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testValue"></param>
    /// <returns></returns>
    [HttpGet("GetUsers/{userId}/{isActive}")] // 表示这个方法响应 GET 请求，并且 URL 路径是 Controller 路径 + /test
    /* public IActionResult Test() */
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUsers_Get";
        string parameters = "";
        DynamicParameters sqlParameters = new DynamicParameters();

        if (userId != 0)
        {
            parameters += ", @UserId=@UserIdParameter" + userId.ToString();
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
        }
        if (isActive)
        {
            parameters += ", @Active=@UserIdParameter" + isActive.ToString();
            sqlParameters.Add("@ActiveParameter", isActive, DbType.Boolean);
        }

        if (parameters.Length > 0)
            sql += parameters.Substring(1); //, parameters.Length);
        System.Console.WriteLine(sql);

        IEnumerable<UserComplete> users = _dapper.LoadDataWithParameter<UserComplete>(
            sql,
            sqlParameters
        );
        return users;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpPut("UpsertUser")]
    public IActionResult UpsertUser(UserComplete user)
    {
        if (_reusableSql.UpsertUser(user))
        {
            return Ok();
        }

        throw new Exception("Failed to Update User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql =
            @"EXEC TutorialAppSchema.spUser_Delete
            @UserId = @UserIdParameter";
        DynamicParameters sqlParameters = new DynamicParameters();
        sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);

        if (_dapper.ExecuteSqlWithParameter(sql, sqlParameters))
        {
            return Ok();
        }
        throw new Exception("Failed to Delete User");
    }
}
