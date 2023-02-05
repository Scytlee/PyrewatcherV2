using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;
using System.Diagnostics;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class ChannelsRepository : IChannelsRepository
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly IDapperWrapper _dapperWrapper;
  private readonly ILogger<ChannelsRepository> _logger;

  public ChannelsRepository(IDbConnectionFactory connectionFactory, IDapperWrapper dapperWrapper, ILogger<ChannelsRepository> logger)
  {
    _connectionFactory = connectionFactory;
    _dapperWrapper = dapperWrapper;
    _logger = logger;
  }

  public async Task<IEnumerable<Channel>> GetConnected()
  {
    const string query = """
SELECT [c].*, [u].[DisplayName], [u].[NormalizedName]
FROM [Core].[Channels] AS [c]
INNER JOIN [Core].[Users] AS [u] ON [u].[Id] = [c].[Id]
WHERE [c].[Connected] = 1;
""";

    using var connection = await _connectionFactory.CreateConnection();

    var stopwatch = Stopwatch.StartNew();
    try
    {
      var result = await _dapperWrapper.QueryAsync<Channel>(connection, query);
      stopwatch.Stop();
      _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(GetConnected), stopwatch.ElapsedMilliseconds);
      return result;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "An error occurred during execution of {MethodName} query", nameof(GetConnected));
      return Array.Empty<Channel>();
    }
  }
}
