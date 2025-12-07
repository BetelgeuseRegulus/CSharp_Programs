using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controller;

[ApiController]
[Route("[Controller]")]
public class UserJobInfoEFController : ControllerBase
{
    DataContextEF _entityFramework;
    UserRepository _userRepository;
    IMapper _mapper;

    public UserJobInfoEFController(IConfiguration config, UserRepository userRepository)
    {
        _entityFramework = new DataContextEF(config);
        _userRepository = userRepository;

        _mapper = new Mapper(
            new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<UserJobInfo, UserJobInfo>();
                },
                null
            )
        );
    }

    // [HttpGet("GetUserJobInfoEF")]
    // public IEnumerable<UserJobInfo> GetUserJobInfo()
    // {
    //     IEnumerable<UserJobInfo> userJobInfos = _entityFramework.UserJobInfo.ToList<UserJobInfo>();
    //     return userJobInfos;
    // }

    [HttpGet("GetSingleUserJobInfoEF/{userId}")]
    public UserJobInfo GetSingleUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfo = _userRepository.GetSingleUserJobInfo(userId);

        if (userJobInfo != null)
            return userJobInfo;

        throw new Exception("Faild to Get UserJobInfo");
    }

    [HttpPut("EditUserJobInfoEF")]
    public IActionResult EditUserJobInfo(UserJobInfo userJobInfo)
    {
        UserJobInfo? userJobInfoToUpdate = _userRepository.GetSingleUserJobInfo(userJobInfo.UserId);

        if (userJobInfoToUpdate != null)
        {
            userJobInfoToUpdate.JobTitle = userJobInfo.JobTitle;
            userJobInfoToUpdate.Department = userJobInfo.Department;

            if (_userRepository.SaveChange())
                return Ok();

            throw new Exception("Failed to Edit UserJobInfo");
        }

        throw new Exception("Failed to Edit UserJobInfo");
    }

    [HttpPost("AddUserJobInfoEF")]
    public IActionResult AddUserJobInfo(UserJobInfo userJobInfo)
    {
        UserJobInfo userJobInfoToAdd = _mapper.Map<UserJobInfo>(userJobInfo);

        //_entityFrameWork.UserJobInfo.Add(userJobInfoDb);
        _userRepository.AddEntity<UserJobInfo>(userJobInfo);
        if (_userRepository.SaveChange())
            return Ok();

        throw new Exception("Failed to Add UserJobInfo");
    }

    [HttpDelete("DeleteUserJobInfoEF/{userId}")]
    public IActionResult DeleteUserJobInfo(int userId)
    {
        UserJobInfo? userJobInfoToDelete = _userRepository.GetSingleUserJobInfo(userId);

        if (userJobInfoToDelete != null)
        {
            //_entityFrameWork.UserJobInfo.Remove(userJobInfoDb);
            _userRepository.RemoveEntity<UserJobInfo>(userJobInfoToDelete);
            if (_userRepository.SaveChange())
                return Ok();

            throw new Exception("Failed to Delete UserJobInfo");
        }

        throw new Exception("Failed to Delete UserJobInfo");
    }
}
