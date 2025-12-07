using AutoMapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controller;

// [] 表示特性(Attribute),给当前类(方法,属性)添加"元数据"(metadata), 启用特性的额外行为.
[ApiController] // 开启 Web API 的智能行为：自动模型验证、自动 400 返回、改进参数绑定
[Route("[Controller]")]
public class UserEFController : ControllerBase
{
    DataContextEF _entityFramework;
    IUserRepository _userRepository;
    IMapper _mapper;

    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
        _entityFramework = new DataContextEF(config);
        _userRepository = userRepository;

        _mapper = new Mapper(
            new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<UserToAddDto, User>();
                },
                null
            )
        );
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="testValue"></param>
    /// <returns></returns>
    [HttpGet("GetUsersEF")] // 表示这个方法响应 GET 请求，并且 URL 路径是 Controller 路径 + /test
    /* public IActionResult Test() */
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _userRepository.GetUsers();
        return users;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("GetSingleUsersEF/{userId}")] // 表示这个方法响应 GET 请求，并且 URL 路径是 Controller 路径 + /test
    /* public IActionResult Test() */
    public User GetSingleUsers(int userId)
    {
        return _userRepository.GetSingleUsers(userId);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpPut("EditUserEF")]
    public IActionResult EditUser(User user)
    {
        User? userToUpdate = _userRepository.GetSingleUsers(user.UserId);

        if (userToUpdate != null)
        {
            userToUpdate.Active = user.Active;
            userToUpdate.FirstName = user.FirstName;
            userToUpdate.LastName = user.LastName;
            userToUpdate.Email = user.Email;
            userToUpdate.Gender = userToUpdate.Gender;

            if (_userRepository.SaveChange())
            {
                return Ok();
            }
            throw new Exception("Failed to Get User");
        }
        throw new Exception("Failed to Get User");
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpPost("AddUserEF")]
    public IActionResult AddUser(UserToAddDto userNoId)
    {
        User userToAdd = _mapper.Map<User>(userNoId);

        _userRepository.AddEntity<User>(userToAdd);
        if (_userRepository.SaveChange())
        {
            return Ok();
        }
        throw new Exception("Failed to Add User");
    }

    [HttpDelete("DeleteUserEF/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        User? userToDelete = _userRepository.GetSingleUsers(userId);

        if (userToDelete != null)
        {
            _entityFramework.Users.Remove(userToDelete);
            if (_userRepository.SaveChange())
            {
                return Ok();
            }

            throw new Exception("Failed to Delete User");
        }

        throw new Exception("Failed to Delete User");
    }
}
