using System.Data;
using System.Security.Cryptography;
using AutoMapper;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Helpers;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
  private readonly DataContextDapper _dapper;
  private readonly AuthHelper _authHelper;
  private readonly ReusableSQL _reusableSQL;
  private readonly IMapper _mapper;

  public AuthController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
    _authHelper = new AuthHelper(config);
    _reusableSQL = new ReusableSQL(config);
    _mapper = new Mapper(new MapperConfiguration(cfg =>
    {
      cfg.CreateMap<UserForRegistrationDto, UsersComplete>();
    }));
  }

  [AllowAnonymous]
  [HttpPost("register")]
  public IActionResult registerUser(UserForRegistrationDto userForRegistration)
  {
    if (userForRegistration.Password != userForRegistration.PasswordConfirm)
    {
      throw new Exception("Passwords do not match");
    }

    string checkUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '" + userForRegistration.Email + "'";
    IEnumerable<string> userExists = _dapper.LoadData<string>(checkUserExists);

    if (userExists.Count() > 0)
    {
      throw new Exception("User already exists");
    }

    byte[] passwordSalt = new byte[128 / 8];
    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
    {
      rng.GetNonZeroBytes(passwordSalt);
    }


    UserForLoginDto userForPassword = new UserForLoginDto()
    {
      Email = userForRegistration.Email,
      Password = userForRegistration.Password
    };

    if (_authHelper.setPassword(userForPassword))
    {
      throw new Exception("Failed to add user");
    }

    UsersComplete user = _mapper.Map<UsersComplete>(userForRegistration);
    user.Active = true;

    if (!_reusableSQL.UpsertUser(user))
    {
      throw new Exception("Failed to add user");
    }

    return Ok();
  }

  [HttpPut("resetPassword")]
  public IActionResult resetPassword(UserForLoginDto userForResetPassword)
  {
    if (_authHelper.setPassword(userForResetPassword))
    {
      throw new Exception("Failed to reset password");
    }
    return Ok();
  }

  [AllowAnonymous]
  [HttpPost("login")]
  public IActionResult loginUser(UserForLoginDto userForLogin)
  {
    string sqlForHash = @"EXEC TutorialAppSchema.spLoginConfirmation_Get 
      @Email = @EmailParam";

    DynamicParameters sqlParameters = new DynamicParameters();
    sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

    UserForLoginConfirmationDto userForLoginConfirmation = _dapper
      .LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHash, sqlParameters);


    byte[] passwordHash = _authHelper.getPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

    for (int i = 0; i < passwordHash.Length; i++)
    {
      if (passwordHash[i] != userForLoginConfirmation.PasswordHash[i])
      {
        return StatusCode(401, "Invalid password");
      }
    }

    int userId = _dapper.LoadDataSingle<int>("SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" + userForLogin.Email + "'");

    return Ok(new Dictionary<string, string> {
      {"token", _authHelper.CreateToken(userId)}
    });
  }

  [HttpGet("RefreshToken")]
  public string RefreshToken()
  {
    string userIdSql = @"SELECT UserId FROM TutorialAppSchema.Users
      WHERE UserId = " + User.FindFirst("userId")?.Value;

    Console.WriteLine(userIdSql);
    int userId = _dapper.LoadDataSingle<int>(userIdSql);
    return _authHelper.CreateToken(userId);
  }
}