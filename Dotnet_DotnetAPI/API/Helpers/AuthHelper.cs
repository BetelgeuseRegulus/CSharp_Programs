using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Dtos;
using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace API.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;
        private readonly DataContextDapper _dapper;

        public AuthHelper(IConfiguration config)
        {
            _config = config;
            _dapper = new DataContextDapper(config);
        }

        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString =
                _config.GetSection("AppSetting:PasswordKey").Value
                + Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 1000000,
                numBytesRequested: 256 / 8
            );
        }

        /// <summary>
        /// 创建 JWT (json Web Token)令牌
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>返回Base64编码的JWT令牌字符串</returns>
        public string CreateToken(int userId)
        {
            // 1.创建一个 Claim 数组
            Claim[] claims = new Claim[] { new Claim("userId", userId.ToString()) };

            // 2.从配置文件中获取安全密钥
            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;
            // 3.创建对称安全密钥
            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(tokenKeyString != null ? tokenKeyString : "")
            );
            // 4.创建安全凭证
            SigningCredentials credentials = new SigningCredentials(
                tokenKey,
                SecurityAlgorithms.HmacSha512Signature
            );
            // 5.创建安全令牌描述符
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1),
            };
            // 6.创建 JWT 安全令牌处理器
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            // 7.根据安全令牌描述符创建安全令牌对象
            SecurityToken token = tokenHandler.CreateToken(descriptor);
            // 8.返回安全令牌
            return tokenHandler.WriteToken(token);
        }

        public bool SetPassword(UserForLoginDto userForSetPassword)
        {
            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = GetPasswordHash(userForSetPassword.Password, passwordSalt);

            System.Console.WriteLine("0x" + BitConverter.ToString(passwordHash).Replace("-", ""));
            System.Console.WriteLine(
                System.Text.Encoding.UTF8.GetString(passwordHash, 0, passwordHash.Length)
            );
            System.Console.WriteLine(System.Text.Encoding.UTF8.GetString(passwordHash));
            System.Console.WriteLine(Convert.ToBase64String(passwordHash));

            string sqlAddAuth =
                @"EXEC TutorialAppSchema.spRegistration_Upsert
            @Email = @EmailParameter,
            @PasswordHash = @PasswordHashParameter,
            @PasswordSalt = @PasswordSaltParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@EmailParameter", userForSetPassword.Email, DbType.String);
            sqlParameters.Add("@PasswordHashParameter", passwordHash, DbType.String);
            sqlParameters.Add("@PasswordSaltParameter", passwordSalt, DbType.String);

            return _dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters);
        }
    }
}
