using System.Data;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers
{
  [Authorize]
  [ApiController]
  [Route("[controller]")]
  public class PostController : ControllerBase
  {

    private readonly DataContextDapper _dapper;
    public PostController(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
    }

    [HttpGet("{id}/{userId}/{search}")]
    public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string search = "None")
    {

      string sql = "EXEC TutorialAppSchema.spPosts_Get";
      string parameters = "";
      DynamicParameters sqlParameters = new DynamicParameters();

      if (postId != 0)
      {
        parameters += ", @PostId=@PostIdParam";
        sqlParameters.Add("@PostIdParam", postId, DbType.Int32);
      }
      if (userId != 0)
      {
        parameters += ", @UserId=@UserIdParam";
        sqlParameters.Add("@UserIdParam", userId, DbType.Int32);
      }
      if (search != "None")
      {
        parameters += ", @SearchText=@SearchTextParam";
        sqlParameters.Add("@SearchTextParam", search, DbType.String);
      }
      if (parameters.Length > 0)
      {
        sql += parameters.Substring(1);
      }

      return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
    }


    [HttpGet("myPosts")]
    public IEnumerable<Post> GetMyPosts()
    {
      string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId = @UserIdParam";

      DynamicParameters sqlParameters = new DynamicParameters();
      sqlParameters.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);

      return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
    }



    [HttpPut()]
    public IActionResult UpsertPost(Post post)
    {
      string sql = @"EXEC TutorialAppSchema.spPosts_Upsert
        @UserId = @UserIdParam, 
        @PostTitle = @PostTitleParam, 
        @PostContent = @PostContentParam";

      DynamicParameters sqlParameters = new DynamicParameters();
      sqlParameters.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
      sqlParameters.Add("@PostTitleParam", post.PostTitle, DbType.String);
      sqlParameters.Add("@PostContentParam", post.PostContent, DbType.String);

      if (post.PostId != 0)
      {
        sql += ", @PostId=@PostIdParam";
        sqlParameters.Add("@PostIdParam", post.PostId, DbType.Int32);
      }

      if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
      {
        return Ok();
      }
      throw new Exception("Failed to update post");
    }

    [HttpDelete("{id}")]
    public IActionResult DeletePost(int id)
    {
      string sql = @"TutorialAppSchema.spPosts_Delete 
        @PostId = @PostIdParam, 
        @UserId = @UserIdParam";

      DynamicParameters sqlParameters = new DynamicParameters();
      sqlParameters.Add("@UserIdParam", this.User.FindFirst("userId")?.Value, DbType.Int32);
      sqlParameters.Add("@PostIdParam", id, DbType.Int32);

      if (_dapper.ExecuteSql(sql))
      {
        return Ok();
      }
      throw new Exception("Failed to delete post");
    }
  }
}
