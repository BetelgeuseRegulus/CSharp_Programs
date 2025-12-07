using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DotnetAPI.Controller;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;

        public AuthHelper(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        /// <summary>
        /// 根据密码和盐值生成密码哈希
        /// </summary>
        /// <param name="password">用户输入的明文密码</param>
        /// <param name="passwordSalt">从数据库获取的用户专属盐值</param>
        /// <returns>返回256位（32字节）的密码哈希值</returns>
        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            // 组合应用级密匙和用户专属盐值
            string passwordSaltPlusString =
                _config.GetSection("AppSettings:PasswordKey").Value
                + Convert.ToBase64String(passwordSalt);

            // 使用PDKDF2算法生成密码哈希
            return KeyDerivation.Pbkdf2(
                password: password, // 用户输入的密码
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString), // 盐值(密码+随机盐)
                prf: KeyDerivationPrf.HMACSHA256, // 使用 HMAC-SHA256伪随机函数
                iterationCount: 1000000, // 迭代次数100000次
                numBytesRequested: 256 / 8 // 生成 256(32位)的哈希值
            );
        }

        /// <summary>
        /// 创建JWT(Json Web Token)令牌
        /// 用于身份验证和授权, 生成有效期为一天的访问令牌
        /// </summary>
        /// <param name="userId">用户ID, </param>
        /// <returns>返回Base64编码的JWT令牌字符串</returns>
        public string CreateToken(int userId)
        {
            // 1.创建一个声明(claim)数组, 将用户ID作为声明添加到令牌中
            Claim[] claims = new Claim[] { new Claim("userId", userId.ToString()) };
            // 2.创建对称安全密钥
            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config.GetSection("AppSettings:TokenKey").Value
                        ?? throw new InvalidOperationException("TokenKey configuration is missing")
                )
            );
            // 3.创建签名凭证
            SigningCredentials credentials = new SigningCredentials(
                tokenKey,
                SecurityAlgorithms.HmacSha512Signature
            );
            // 4.创建安全令牌描述符
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims), // 令牌主体: 包含用户的令牌信息
                SigningCredentials = credentials, // 签名凭证: 用于于令牌签名
                Expires = DateTime.Now.AddDays(1), // 过期时间: 从当前时间起24小时后过期
                /* 可选属性（当前未使用）：*/
                // Issuer = "your-app-name",            // 令牌颁发者
                // Audience = "your-api",               // 令牌受众（目标API）
                // NotBefore = DateTime.Now,            // 令牌生效时间
                // IssuedAt = DateTime.Now              // 令牌颁发时间
            };
            ;
            // 5.创建JWT令牌处理器
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            // 6. 根据描述符创建安全令牌对象
            SecurityToken token = tokenHandler.CreateToken(descriptor);

            // 7.将令牌对象序列化为Base64编码的字符串
            // 返回的字符串格式为: header.payload.signture
            return tokenHandler.WriteToken(token);
        }

        public bool SetPassword(UserForLoginDto userForSetPassword)
        {
            // 3.生成密码盐值
            // 创建128位(16字节)的随机盐值
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            // 5.使用 PBKDF2 算法生成密码哈希
            byte[] passwordHash = GetPasswordHash(userForSetPassword.Password, passwordSalt);

            // 6.构建 SQL 插入语句
            string sqlAddAuth =
                @"EXEC TutorialAppSchema.spRegistration_Upsert
                        @Email = @EmailParam,
                        @PasswordHash = @PasswordHashParam, 
                        @PasswordSalt = @PasswordSaltParam";

            System.Console.WriteLine(sqlAddAuth);

            //7.创建SQL参数列表(防止哈希和盐值的SQL注入)
            //List<SqlParameter> sqlParameters = new List<SqlParameter>();
            // 添加密码邮件参数
            // SqlParameter passwordEmailParameter = new SqlParameter(
            //     @"EmailParam",
            //     SqlDbType.VarChar
            // );
            // passwordEmailParameter.Value = userForSetPassword.Email;
            // sqlParameters.Add(passwordEmailParameter);
            // 添加密码盐值参数
            // SqlParameter passwordSaltParameter = new SqlParameter(
            //     "@PasswordSalt",
            //     SqlDbType.VarBinary
            // );
            // passwordSaltParameter.Value = passwordSalt;
            // sqlParameters.Add(passwordSaltParameter);
            //添加密码哈希参数
            // SqlParameter passwordHashParameter = new SqlParameter(
            //     "@PasswordHash",
            //     SqlDbType.VarBinary
            // );
            // passwordHashParameter.Value = passwordHash;
            // sqlParameters.Add(passwordHashParameter);

            // 使用动态参数添加 邮件, 密码盐值, 密码哈希参数
            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@EmailParam", userForSetPassword.Email, DbType.String);
            sqlParameters.Add("@PasswordHashParam", passwordHash, DbType.Binary);
            sqlParameters.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);

            //8.执行 SQL插入操作
            return _dapper.ExecuteSqlWithParameter(sqlAddAuth, sqlParameters);
        }
    }
}
