using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controller;

[ApiController]
[Route("[Controller]")]
public class UserJobInfoController : ControllerBase
{
    DataContextDapper _dapper;

    public UserJobInfoController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("GetUserJobInfo")]
    public IEnumerable<UserJobInfo> GetUserJobInfos()
    {
        string sql = @"
                        SELECT 
                            [UserId],
                            [JobTitle],
                            [Department] 
                            FROM TutorialAppSchema.UserJobInfo
                            ";

        IEnumerable<UserJobInfo> userJobInfos = _dapper.LoadData<UserJobInfo>(sql);
        return userJobInfos;
    }

    [HttpGet("GetSingleUsersJobInfo/{userId}")]
    public UserJobInfo GetSingleUserJobInfo(int userId)
    {
        string sql = @"
                    SELECT 
                        [UserId],
                        [JobTitle],
                        [Department] 
                        FROM TutorialAppSchema.UserJobInfo
                        WHERE UserId = " + userId.ToString();

        UserJobInfo userJobInfo = _dapper.LoadDataSingle<UserJobInfo>(sql);
        return userJobInfo;
    }


    [HttpPost("AddUserJobInfo")]
    public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
    {
        string sql = @"
                    INSERT INTO TutorialAppSchema.UserJobInfo(
                        [UserId],
                        [JobTitle],
                        [Department]
                    ) 
                    VALUES" +
                   "(" +
                      "'" + userJobInfo.UserId + "'," +
                      "'" + userJobInfo.JobTitle + "'," +
                      "'" + userJobInfo.Department + "'" +
                      ")";

        System.Console.WriteLine(sql);
        if (_dapper.ExecuteSql(sql))
            return Ok();

        throw new Exception("Failed to Add UserJobInfo");
    }


    [HttpPut("EditUserJobInfo")]
    public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
    {
        string sql = @"
            UPDATE TutorialAppSchema.UserJobInfo
                SET" +
                    "[JobTitle] = '" + userJobInfo.JobTitle + "'," +
                    "[Department] = '" + userJobInfo.Department + "'" +
                "WHERE UserId = " + userJobInfo.UserId.ToString();

        System.Console.WriteLine(sql);
        if (_dapper.ExecuteSql(sql))
            return Ok();

        throw new Exception("Failed to Edit UserJobInfo");
    }


    [HttpDelete("DeleteUserJobInfo/{userId}")]
    public IActionResult DeleteUserJobInfo(int userId)
    {
        string sql = @"DELETE TutorialAppSchema.UserJobInfo WHERE UserId = " + userId.ToString();

        if (_dapper.ExecuteSql(sql))
            return Ok();

        throw new Exception("Failed to delete UserJobInfo");
    }
}