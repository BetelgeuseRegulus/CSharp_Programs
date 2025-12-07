using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using API.Helpers;
using API.Models;
using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[Controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        private readonly ReusableSql _reusableSql;
        private readonly IMapper _mapper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSql(config);
            _mapper = new Mapper(
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.CreateMap<UserForRegistrationDto, UserComplete>();
                    },
                    null
                )
            );
        }

        /// <summary>
        /// 用户注册接口
        /// </summary>
        /// <param name="userForRegistration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists =
                    "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '"
                    + userForRegistration.Email
                    + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    UserForLoginDto userForSetPassword = new UserForLoginDto()
                    {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password,
                    };
                    if (_authHelper.SetPassword(userForSetPassword))
                    {
                        UserComplete userComplete = _mapper.Map<UserComplete>(userForSetPassword);
                        userComplete.Active = true;

                        if (_reusableSql.UpsertUser(userComplete))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user");
                    }
                    throw new Exception("Failed to register user");
                }
                throw new Exception("User with this email already exists");
            }
            throw new Exception("Passwords do not match");
        }

        /// <summary>
        /// 用户登录接口
        /// </summary>
        /// <param name="userForLogin"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt =
                @"EXEC TutorialAppSchema.spLoginConfirmation_Get
                      @Email = @EmailParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@EmailParameter", userForLogin.Email, DbType.String);

            UserForLoginConfirmationDto userForLoginConfirmation =
                _dapper.LoadDataSingleWithParameters<UserForLoginConfirmationDto>(
                    sqlForHashAndSalt,
                    sqlParameters
                );

            byte[] passwordHash = _authHelper.GetPasswordHash(
                userForLogin.Password,
                userForLoginConfirmation.PasswordSalt
            );

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForLoginConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, "Incorrect password");
                }
            }

            string userIdSql =
                @"SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '"
                + userForLogin.Email
                + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(
                new Dictionary<string, string> { { "token", _authHelper.CreateToken(userId) } }
            );
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string userIdSql =
                @"SELECT FROM TutorialAppSchema.Users WHERE UserId = '"
                + User.FindFirst("userId")?.Value
                + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return _authHelper.CreateToken(userId);
        }
    }
}
