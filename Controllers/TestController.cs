using DotNetAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace DotNetAPI.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class TestController : ControllerBase
  {

    private readonly DataContextDapper _dapper;
    public TestController(IConfiguration config)
    {
      _dapper = new DataContextDapper(config);
    }

    [HttpGet()]
    public string Test()
    {
      return "Application is running";
    }

    [HttpGet("connection")]
    public DateTime TestConnection()
    {
      return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }
  }
}
