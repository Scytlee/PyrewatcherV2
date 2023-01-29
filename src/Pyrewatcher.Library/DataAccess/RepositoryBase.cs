using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Pyrewatcher.Library.DataAccess;

public abstract class RepositoryBase
{
  private readonly IConfiguration _config;

  protected RepositoryBase(IConfiguration config)
  {
    _config = config;
  }

  protected async Task<IDbConnection> CreateConnection()
  {
    var conn = new SqlConnection(_config.GetConnectionString("Database"));
    await conn.OpenAsync();

    return conn;
  }
}