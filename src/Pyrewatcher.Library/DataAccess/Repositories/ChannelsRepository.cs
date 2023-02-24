using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class ChannelsRepository : IChannelsRepository
{
  private readonly IDapperService _dapperService;
  private readonly ILogger<ChannelsRepository> _logger;

  public ChannelsRepository(IDapperService dapperService, ILogger<ChannelsRepository> logger)
  {
    _dapperService = dapperService;
    _logger = logger;
  }

  public async Task<Result<IEnumerable<Channel>>> GetConnected()
  {
    const string query = """
SELECT [c].*, [u].[DisplayName], [u].[NormalizedName]
FROM [Core].[Channels] AS [c]
INNER JOIN [Core].[Users] AS [u] ON [u].[Id] = [c].[Id]
WHERE [c].[Connected] = 1;
""";

    var dbResult = await _dapperService.QueryAsync<Channel>(query);
    if (!dbResult.IsSuccess)
    {
      _logger.LogError(dbResult.Exception, "An error occurred during execution of {MethodName} query", nameof(GetConnected));
      return Result<IEnumerable<Channel>>.Failure(Array.Empty<Channel>());
    }
    
    _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(GetConnected), dbResult.ExecutionTime);
    return Result<IEnumerable<Channel>>.Success(dbResult.Content);
  }
}
