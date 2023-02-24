using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class CommandAliasesRepository : ICommandAliasesRepository
{
  private readonly IDapperService _dapperService;
  private readonly ILogger<CommandAliasesRepository> _logger;

  public CommandAliasesRepository(IDapperService dapperService, ILogger<CommandAliasesRepository> logger)
  {
    _dapperService = dapperService;
    _logger = logger;
  }

  public async Task<Result<string?>> GetNewCommandByAliasAndChannelId(string alias, long channelId)
  {
    const string query = """
SELECT [NewCommand]
FROM [Command].[Aliases]
WHERE [Alias] = @alias AND ([ChannelId] IS NULL OR [ChannelId] = @channelId);
""";

    var dbResult = await _dapperService.QuerySingleOrDefaultAsync<string?>(query, new { alias, channelId });
    if (!dbResult.IsSuccess)
    {
      _logger.LogError(dbResult.Exception, "An error occurred during execution of {MethodName} query", nameof(GetNewCommandByAliasAndChannelId));
      return Result<string?>.Failure();
    }
    
    _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(GetNewCommandByAliasAndChannelId), dbResult.ExecutionTime);
    return Result<string?>.Success(dbResult.Content);
  }
}
