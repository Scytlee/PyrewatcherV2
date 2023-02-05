using Pyrewatcher.Library.DataAccess.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace Pyrewatcher.Library.DataAccess.Factories;

public class SqlConnectionFactory : IDbConnectionFactory
{
  private readonly string _connectionString;

  public SqlConnectionFactory(string connectionString)
  {
    _connectionString = connectionString;
  }

  public async Task<IDbConnection> CreateConnection()
  {
    var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    return connection;
  }
}
