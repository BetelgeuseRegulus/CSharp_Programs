
using System.Data;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Controller
{
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        /// <summary>
        /// 用户注册接口
        /// </summary>
        /// <param name="userForRegistration">用户注册信息DTO, 包含邮箱,密码和确认密码</param>
        /// <returns>注册成功返回200 OK, 失败抛出异常</returns>
        /// <exception cref="Exception">密码不匹配,用户已存在或者用户失败时抛出异常</exception>
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            // 1. 验证密码和确认密码是否匹配
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                //2.检查邮箱是否已经注册
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);

                // 如果该邮箱未注册
                if (existingUsers.Count() == 0)
                {
                    // 3.生成密码盐值
                    // 创建128位(16字节)的随机盐值
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    // 4.将配置文件中的密匙与盐值结合
                    string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

                    // 5.使用 PBKDF2 算法生成密码哈希
                    byte[] passwordHash = GetPasswordHash(userForRegistration.Password, passwordSalt);


                    // 6.构建 SQL 插入语句
                    string sqlAddAuth = @"
                    INSERT INTO TutorialAppSchema.Auth 
                    (
                    [Email],
                    [PasswordHash],
                    [PasswordSalt]
                    )
                    VALUES('" + userForRegistration.Email + "'," +
                    "@PasswordHash, @PasswordSalt)";

                    System.Console.WriteLine(sqlAddAuth);

                    //7.创建SQL参数列表(防止哈希和盐值的SQL注入)
                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    // 添加密码盐值参数
                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;
                    //添加密码哈希参数
                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;

                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);

                    //8.执行 SQL插入操作
                    if (_dapper.ExecuteSqlWithParameter(sqlAddAuth, sqlParameters))
                        return Ok();

                    throw new Exception("Failed to register user.");        // 插入失败,抛出异常
                }
                throw new Exception("User with this email already exists!");// 邮箱已被注册,抛出异常
            }
            throw new Exception("Passwords do not match!");                 // 密码不匹配,抛出异常
        }


        /// <summary>
        /// 用户登录接口
        /// </summary>
        /// <param name="userForLoginDto">用户登录信息DTO, 包含邮箱和密码</param>
        /// <returns>登录结果</returns>
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"
                                        SELECT 
                                            [PasswordHash],
                                            [PasswordSalt]
                                            FROM TutorialAppSchema.Auth WHERE Email = '" + userForLogin.Email + "'";

            UserForLoginConfirmationDto userForLoginConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            byte[] passwordHash = GetPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

            //if(passwordHash == userForConfirmation.PasswordHash) => Won't work
            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForLoginConfirmation.PasswordHash[index])
                {
                    return StatusCode(401, "Incorrect password");
                }
            }

            return Ok();
        }

        /// <summary>
        /// 根据密码和盐值生成密码哈希
        /// </summary>
        /// <param name="password">用户输入的明文密码</param>
        /// <param name="passwordSalt">用户专属盐值</param>
        /// <returns>密码哈希值</returns>
        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                                        password: password,                                     // 用户输入的密码
                                        salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),  // 盐值(密码+随机盐)
                                        prf: KeyDerivationPrf.HMACSHA256,                       // 使用 HMAC-SHA256伪随机函数
                                        iterationCount: 1000000,                                // 迭代次数100000次
                                        numBytesRequested: 256 / 8                              // 生成 256(32位)的哈希值
                                       );
        }
    }
}