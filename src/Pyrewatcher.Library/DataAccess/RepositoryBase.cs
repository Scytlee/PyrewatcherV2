using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Pyrewatcher.Library.DataAccess;

public abstract class RepositoryBase
{
  private readonly IConfiguration _configuration;

  protected RepositoryBase(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  protected async Task<IDbConnection> CreateConnection()
  {
    var conn = new SqlConnection(_configuration.GetConnectionString("Database"));
    await conn.OpenAsync();

    return conn;
  }
}