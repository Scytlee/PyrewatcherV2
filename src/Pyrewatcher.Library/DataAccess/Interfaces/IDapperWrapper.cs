using System.Data;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IDapperWrapper
{
  Task<int> ExecuteAsync(IDbConnection connection, string sql, object? parameters = null);

  Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string sql, object? parameters = null);

  Task<T> QuerySingleAsync<T>(IDbConnection connection, string sql, object? parameters = null);

  Task<T?> QuerySingleOrDefaultAsync<T>(IDbConnection connection, string sql, object? parameters = null);
}
