using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotNetAPI.Helpers
{
  public class AuthHelper
  {

    private readonly DataContextDapper _dapper;

    private readonly IConfiguration _config;
    public AuthHelper(IConfiguration config)
    {
      _config = config;
      _dapper = new DataContextDapper(config);
    }

    public byte[] getPasswordHash(string password, byte[] passwordSalt)
    {
      string passwordSaltString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);

      return KeyDerivation.Pbkdf2(
        password: password,
        salt: Encoding.ASCII.GetBytes(passwordSaltString),
        prf: KeyDerivationPrf.HMACSHA256,
        iterationCount: 10000,
        numBytesRequested: 256 / 8
      );
    }

    public string CreateToken(int userId)
    {
      Claim[] claims = new Claim[] {
      new Claim("userId", userId.ToString())
     };

      SymmetricSecurityKey key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:TokenKey").Value)
      );

      SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

      SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(1),
        SigningCredentials = creds
      };

      JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

      SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }

    public bool setPassword(UserForLoginDto userSetPassword)
    {
      byte[] passwordSalt = new byte[128 / 8];
      using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
      {
        rng.GetNonZeroBytes(passwordSalt);
      }

      byte[] passwordHash = getPasswordHash(userSetPassword.Password, passwordSalt);

      string sqlAddAuth = @"EXEC TutorialAppSchema.spRegistration_Upsert 
      @Email = @EmailParam,
      @PasswordHash = @PasswordHashParam,
      @PasswordSalt = @PasswordSaltParam";

      DynamicParameters sqlParameters = new DynamicParameters();
      sqlParameters.Add("@EmailParam", userSetPassword.Email, DbType.String);
      sqlParameters.Add("@PasswordSaltParam", passwordSalt, DbType.Binary);
      sqlParameters.Add("@PasswordHashParam", passwordHash, DbType.Binary);

      return _dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters);
    }
  }
}
