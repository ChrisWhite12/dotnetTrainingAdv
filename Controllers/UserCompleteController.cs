using System.Data;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Helpers;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserCompleteController : ControllerBase
{
  private readonly DataContextDapper _dapper;
  private readonly ReusableSQL _reusableSQL;
  public UserCompleteController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
    _reusableSQL = new ReusableSQL(config);
  }

  [HttpGet("{id}/{active}")]
  public IEnumerable<UsersComplete> GetUsers(int id, bool active)
  {
    string sql = "EXEC TutorialAppSchema.spUsers_Get";
    string parameters = "";
    DynamicParameters sqlParameters = new DynamicParameters();

    if (id != 0)
    {
      parameters += ", @UserId = @UserIdParam";
      sqlParameters.Add("@UserIdParam", id, DbType.Int32);
    }
    if (active)
    {
      parameters += ", @Active = @ActiveParam";
      sqlParameters.Add("@ActiveParam", active, DbType.Boolean);
    }
    if (parameters.Length > 0)
    {
      sql += parameters.Substring(1);
    }

    return _dapper.LoadDataWithParameters<UsersComplete>(sql, sqlParameters);
  }

  [HttpPut()]
  public IActionResult UpsertUser(UsersComplete user)
  {
    if (_reusableSQL.UpsertUser(user))
    {

      return Ok();
    }
    throw new Exception("Failed to update user");
  }

  [HttpDelete("{id}")]
  public IActionResult DeleteUser(int id)
  {
    string sql = @"EXEC TutorialAppSchema.spUser_Delete @UserId = @UserIdParam";

    DynamicParameters sqlParameters = new DynamicParameters();
    sqlParameters.Add("@UserIdParam", id, DbType.Int32);

    if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
    {
      return Ok();
    }
    throw new Exception("Failed to delete user");
  }
}
