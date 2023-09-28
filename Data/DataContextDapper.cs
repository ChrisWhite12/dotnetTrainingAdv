using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotNetAPI.Data
{
  class DataContextDapper
  {
    private readonly IConfiguration _config;
    public DataContextDapper(IConfiguration config)
    {
      _config = config;
    }

    public IEnumerable<T> LoadData<T>(string sql)
    {
      string connectionString = _config.GetConnectionString("DefaultConnection");
      using (IDbConnection connection = new SqlConnection(connectionString))
      {
        return connection.Query<T>(sql);
      }
    }

    public T LoadDataSingle<T>(string sql)
    {
      string connectionString = _config.GetConnectionString("DefaultConnection");
      using (IDbConnection connection = new SqlConnection(connectionString))
      {
        return connection.QuerySingle<T>(sql);
      }
    }

    public bool ExecuteSql(string sql)
    {
      string connectionString = _config.GetConnectionString("DefaultConnection");
      using (IDbConnection connection = new SqlConnection(connectionString))
      {
        return connection.Execute(sql) > 0;
      }
    }

    public int ExecuteSqlWithRowCount(string sql)
    {
      string connectionString = _config.GetConnectionString("DefaultConnection");
      using (IDbConnection connection = new SqlConnection(connectionString))
      {
        return connection.Execute(sql);
      }
    }

    public bool ExecuteSqlWithParameters(string sql, DynamicParameters parameters)
    {
      string connectionString = _config.GetConnectionString("DefaultConnection");
      using (IDbConnection connection = new SqlConnection(connectionString))
      {
        return connection.Execute(sql, parameters) > 0;
      }
    }

    public IEnumerable<T> LoadDataWithParameters<T>(string sql, DynamicParameters parameters)
    {
      string connectionString = _config.GetConnectionString("DefaultConnection");
      using (IDbConnection connection = new SqlConnection(connectionString))
      {
        return connection.Query<T>(sql, parameters);
      }
    }

    public T LoadDataSingleWithParameters<T>(string sql, DynamicParameters parameters)
    {
      string connectionString = _config.GetConnectionString("DefaultConnection");
      using (IDbConnection connection = new SqlConnection(connectionString))
      {
        return connection.QuerySingle<T>(sql, parameters);
      }
    }
  }
}