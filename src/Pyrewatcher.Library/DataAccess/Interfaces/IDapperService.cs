using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IDapperService
{
  Task<DbResult<int>> ExecuteAsync(string sql, object? parameters = null);

  Task<DbResult<IEnumerable<T>>> QueryAsync<T>(string sql, object? parameters = null);

  Task<DbResult<T>> QuerySingleAsync<T>(string sql, object? parameters = null);

  Task<DbResult<T?>> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null);
}
