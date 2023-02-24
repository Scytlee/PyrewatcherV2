using Dapper;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;
using System.Diagnostics;

namespace Pyrewatcher.Library.DataAccess.Services;

public class DapperService : IDapperService
{
  private static Stopwatch NewStopwatch => Stopwatch.StartNew();

  private readonly IDbConnectionFactory _connectionFactory;

  public DapperService(IDbConnectionFactory connectionFactory)
  {
    _connectionFactory = connectionFactory;
  }

  public async Task<DbResult<int>> ExecuteAsync(string sql, object? parameters = null)
  {
    using var connection = await _connectionFactory.CreateConnection();
    
    try
    {
      var stopwatch = NewStopwatch;
      var result = await connection.ExecuteAsync(sql, parameters);
      stopwatch.Stop();
      return DbResult<int>.Success(result, stopwatch.ElapsedMilliseconds);
    }
    catch (Exception exception)
    {
      return DbResult<int>.Failure(exception);
    }
  }

  public async Task<DbResult<IEnumerable<T>>> QueryAsync<T>(string sql, object? parameters = null)
  {
    using var connection = await _connectionFactory.CreateConnection();
    
    try
    {
      var stopwatch = NewStopwatch;
      var result = await connection.QueryAsync<T>(sql, parameters);
      stopwatch.Stop();
      return DbResult<IEnumerable<T>>.Success(result, stopwatch.ElapsedMilliseconds);
    }
    catch (Exception exception)
    {
      return DbResult<IEnumerable<T>>.Failure(exception);
    }
  }

  public async Task<DbResult<T>> QuerySingleAsync<T>(string sql, object? parameters = null)
  {
    using var connection = await _connectionFactory.CreateConnection();
    
    try
    {
      var stopwatch = NewStopwatch;
      var result = await connection.QuerySingleAsync<T>(sql, parameters);
      stopwatch.Stop();
      return DbResult<T>.Success(result, stopwatch.ElapsedMilliseconds);
    }
    catch (Exception exception)
    {
      return DbResult<T>.Failure(exception);
    }
  }

  public async Task<DbResult<T?>> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null)
  {
    using var connection = await _connectionFactory.CreateConnection();
    
    try
    {
      var stopwatch = NewStopwatch;
      var result = await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
      stopwatch.Stop();
      return DbResult<T?>.Success(result, stopwatch.ElapsedMilliseconds);
    }
    catch (Exception exception)
    {
      return DbResult<T?>.Failure(exception);
    }
  }
}
