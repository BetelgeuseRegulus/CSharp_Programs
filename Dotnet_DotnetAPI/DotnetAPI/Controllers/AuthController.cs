using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controller
{
    [ApiController]
    [Route("[controller]")]
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
                    cfg => cfg.CreateMap<UserForRegistrationDto, UserComplete>(),
                    null
                )
            );
        }

        /// <summary>
        /// 用户注册接口
        /// </summary>
        /// <param name="userForRegistration">用户注册信息DTO, 包含邮箱,密码和确认密码</param>
        /// <returns>注册成功返回200 OK, 失败抛出异常</returns>
        /// <exception cref="Exception">密码不匹配,用户已存在或者用户失败时抛出异常</exception>
        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            // 1. 验证密码和确认密码是否匹配
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                //2.检查邮箱是否已经注册
                string sqlCheckUserExists =
                    "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '"
                    + userForRegistration.Email
                    + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);

                // 如果该邮箱未注册
                if (existingUsers.Count() == 0)
                {
                    UserForLoginDto userForSetPassword = new UserForLoginDto()
                    {
                        Email = userForRegistration.Email,
                        Password = userForRegistration.Password,
                    };

                    //8.执行 SQL插入操作
                    if (_authHelper.SetPassword(userForSetPassword))
                    {
                        UserComplete userComplete = _mapper.Map<UserComplete>(userForRegistration);

                        if (_reusableSql.UpsertUser(userComplete))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user"); // 新用户添加失败, 抛出异常
                    }
                    throw new Exception("Failed to register user."); // 插入失败,抛出异常
                }
                throw new Exception("User with this email already exists!"); // 邮箱已被注册,抛出异常
            }
            throw new Exception("Passwords do not match!"); // 密码不匹配,抛出异常
        }

        /// <summary>
        /// 用户修改密码接口
        /// </summary>
        /// <param name="userForSetPassword"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForSetPassword)
        {
            if (_authHelper.SetPassword(userForSetPassword))
            {
                return Ok();
            }
            throw new Exception("Failed to update password");
        }

        /// <summary>
        /// 用户登录接口
        /// </summary>
        /// <param name="userForLoginDto">用户登录信息DTO, 包含邮箱和密码</param>
        /// <returns>登录结果</returns>
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            // 1. 构建 SQL 查询语句, 从数据库获取该邮箱对应的密码哈希和盐值
            string sqlForHashAndSalt =
                @"TutorialAppSchema.spLoginConfirmation_GET @Email=@EmailParam";

            // List<SqlParameter> sqlParameters = new List<SqlParameter>();
            // SqlParameter emailParameter = new SqlParameter("@EmailParam", SqlDbType.VarChar);
            // emailParameter.Value = userForLogin.Email;
            // sqlParameters.Add(emailParameter);

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

            // 2. 执行查询,获取单条用户认证信息
            UserForLoginConfirmationDto userForLoginConfirmation =
                _dapper.LoadDataSingleWithParameter<UserForLoginConfirmationDto>(
                    sqlForHashAndSalt,
                    sqlParameters
                );

            // 3. 使用数据库中的盐值, 对用户输入的密码进行哈希计算
            byte[] passwordHash = _authHelper.GetPasswordHash(
                userForLogin.Password,
                userForLoginConfirmation.PasswordSalt
            );

            //if(passwordHash == userForConfirmation.PasswordHash) => Won't work 比较的是引用地址而不是字节内容
            // 4. 逐字节比对计算出的哈希值与数据库中存储的哈希值
            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForLoginConfirmation.PasswordHash[index])
                {
                    // 返回 401 未授权状态码,提示密码错误
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

        /// <summary>
        /// 刷新 JWT 令牌接口
        /// 用于在旧令牌即将过期时，为已登录的用户生成新的JWT令牌
        /// 要求用户必须持有有效的JWT令牌才能访问此接口（需要[Authorize]特性）
        /// </summary>
        /// <returns></returns>
        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            // 1.从当前 HTTP 请求中的用户声明(Claims)中提取用户ID
            // 2.构建 SQL 查询语句,验证用户ID在数据库中是否存在
            string userIdSql =
                "SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = '"
                + User.FindFirst("userId")?.Value
                + "'";
            System.Console.WriteLine("userIdSql = " + userIdSql);

            // 3.执行 SQL 查询, 从数据库获取用户ID
            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            // 4.使用从数据库验证后的用户ID生成新的 JWT 令牌
            return _authHelper.CreateToken(userId);
        }
    }

    internal class DynamicaParameters { }
}
