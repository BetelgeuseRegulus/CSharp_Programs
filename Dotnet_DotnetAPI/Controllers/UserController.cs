using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controller;

// [] 表示特性(Attribute),给当前类(方法,属性)添加"元数据"(metadata), 启用特性的额外行为.
[ApiController] // 开启 Web API 的智能行为：自动模型验证、自动 400 返回、改进参数绑定
[Route("[Controller]")]
public class UserController : ControllerBase
{
    DataContextDapper _dapper;

    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
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
    [HttpGet("GetUsers")] // 表示这个方法响应 GET 请求，并且 URL 路径是 Controller 路径 + /test
    /* public IActionResult Test() */
    public IEnumerable<User> GetUsers()
    {
        string sql = @"
                        SELECT [UserId],       
                            [FirstName],
                            [LastName],
                            [Email],
                            [Gender],
                            [Active]
                        FROM TutorialAppSchema.Users"
                    ;
        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        return users;
        // string[] responseArray = new string[] { "user1", "user2" };
        // return responseArray;
    }

    [HttpGet("GetSingleUsers/{userId}")] // 表示这个方法响应 GET 请求，并且 URL 路径是 Controller 路径 + /test
    /* public IActionResult Test() */
    public User GetSingleUsers(int userId)
    {
        // string[] responseArray = new string[] { "test1", "test2", testValue };
        // return responseArray;

        string sql = @"
                        SELECT [UserId],       
                               [FirstName],
                               [LastName],
                               [Email],
                               [Gender],
                               [Active]
                        FROM TutorialAppSchema.Users
                           WHERE UserId = " + userId.ToString(); // "7"
        User user = _dapper.LoadDataSingle<User>(sql);
        return user;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPut("EditUser")]
    public IActionResult EditUser(User user)
    {
        string sql = @"
                        UPDATE TutorialAppSchema.Users
                            SET [FirstName] = '" + user.FirstName + "'," +
                                "[LastName] = '" + user.LastName + "'," +
                                "[Email] = '" + user.Email + "'," +
                                "[Gender] = '" + user.Gender + "'," +
                                "[Active] = '" + user.Active + "'" +
                           " WHERE UserId = " + user.UserId;

        System.Console.WriteLine(sql);
        if (_dapper.ExecuteSql(sql))
            return Ok();

        throw new Exception("Failed to Update User");
    }


    /// <summary>   
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string sql = @"
        INSERT INTO TutorialAppSchema.Users(
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
        ) VALUES ( " +
           "'" + user.FirstName + "'," +
           "'" + user.LastName + "'," +
           "'" + user.Email + "'," +
           "'" + user.Gender + "'," +
           "'" + user.Active + "'" +
           ")";

        System.Console.WriteLine(sql);
        if (_dapper.ExecuteSql(sql))
            return Ok();

        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"DELETE FROM TutorialAppSchema.Users WHERE UserId = " + userId.ToString();
        System.Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Delete User");
    }
}

