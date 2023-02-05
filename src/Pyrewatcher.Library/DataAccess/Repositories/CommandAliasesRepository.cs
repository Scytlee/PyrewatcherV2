using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using System.Diagnostics;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class CommandAliasesRepository : ICommandAliasesRepository
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly IDapperWrapper _dapperWrapper;
  private readonly ILogger<CommandAliasesRepository> _logger;

  public CommandAliasesRepository(IDbConnectionFactory connectionFactory, IDapperWrapper dapperWrapper, ILogger<CommandAliasesRepository> logger)
  {
    _connectionFactory = connectionFactory;
    _dapperWrapper = dapperWrapper;
    _logger = logger;
  }

  public async Task<string?> GetNewCommandByAliasAndChannelId(string alias, long channelId)
  {
    const string query = """
SELECT [NewCommand]
FROM [Command].[Aliases]
WHERE [Alias] = @alias AND ([ChannelId] IS NULL OR [ChannelId] = @channelId);
""";

    using var connection = await _connectionFactory.CreateConnection();

    var stopwatch = Stopwatch.StartNew();
    try
    {
      var result = await _dapperWrapper.QuerySingleOrDefaultAsync<string?>(connection, query, new { alias, channelId });
      stopwatch.Stop();
      _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(GetNewCommandByAliasAndChannelId), stopwatch.ElapsedMilliseconds);
      return result;
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "An error occurred during execution of {MethodName} query", nameof(GetNewCommandByAliasAndChannelId));
      return null;
    }
  }
}
