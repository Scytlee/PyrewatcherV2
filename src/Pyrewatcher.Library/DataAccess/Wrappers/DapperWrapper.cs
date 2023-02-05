using Dapper;
using Pyrewatcher.Library.DataAccess.Interfaces;
using System.Data;

namespace Pyrewatcher.Library.DataAccess.Wrappers;

public class DapperWrapper : IDapperWrapper
{
  public async Task<int> ExecuteAsync(IDbConnection connection, string sql, object? parameters = null)
  {
    return await connection.ExecuteAsync(sql, parameters);
  }

  public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? parameters = null)
  {
    return await connection.QueryAsync<T>(sql, parameters);
  }

  public async Task<T> QuerySingleAsync<T>(IDbConnection connection, string sql, object? parameters = null)
  {
    return await connection.QuerySingleAsync<T>(sql, parameters);
  }

  public async Task<T?> QuerySingleOrDefaultAsync<T>(IDbConnection connection, string sql, object? parameters = null)
  {
    return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
  }
}
