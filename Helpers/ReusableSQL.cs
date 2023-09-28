using System.Data;
using Dapper;
using DotNetAPI.Data;
using DotNetAPI.Models;

namespace DotNetAPI.Helpers
{
  public class ReusableSQL
  {
    private readonly DataContextDapper _dapper;
    public ReusableSQL(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);

    }

    public bool UpsertUser(UsersComplete user)
    {
      string sql = @"EXEC TutorialAppSchema.spUser_Upsert
        @FirstName = @FirstNameParam, 
        @LastName = @LastNameParam, 
        @Email = @EmailParam, 
        @Gender = @GenderParam, 
        @Active = @ActiveParam, 
        @JobTitle = @JobTitleParam, 
        @Department = @DepartmentParam, 
        @Salary = @SalaryParam, 
        @UserId = @UserIdParam";


      DynamicParameters sqlParameters = new DynamicParameters();
      sqlParameters.Add("@UserIdParam", user.UserId, DbType.Int32);
      sqlParameters.Add("@FirstNameParam", user.FirstName, DbType.String);
      sqlParameters.Add("@LastNameParam", user.LastName, DbType.String);
      sqlParameters.Add("@EmailParam", user.Email, DbType.String);
      sqlParameters.Add("@GenderParam", user.Gender, DbType.String);
      sqlParameters.Add("@ActiveParam", user.Active, DbType.Boolean);
      sqlParameters.Add("@JobTitleParam", user.JobTitle, DbType.String);
      sqlParameters.Add("@DepartmentParam", user.Department, DbType.String);
      sqlParameters.Add("@SalaryParam", user.Salary, DbType.Decimal);

      return _dapper.ExecuteSqlWithParameters(sql, sqlParameters);
    }
  }
}