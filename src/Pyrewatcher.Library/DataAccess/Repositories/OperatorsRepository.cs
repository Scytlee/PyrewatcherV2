using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Enums;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class OperatorsRepository : IOperatorsRepository
{
  private readonly IDapperService _dapperService;
  private readonly ILogger<OperatorsRepository> _logger;

  public OperatorsRepository(IDapperService dapperService, ILogger<OperatorsRepository> logger)
  {
    _dapperService = dapperService;
    _logger = logger;
  }

  public async Task<Result<ChatRoles>> GetUsersOperatorRoleByChannel(long userId, long channelId)
  {
    const string query = """
DECLARE @role VARCHAR(16) = (
  SELECT TOP 1 CASE WHEN [ChannelId] IS NULL THEN 'GlobalOperator' ELSE 'ChannelOperator' END
  FROM [Core].[Operators]
  WHERE [UserId] = @userId AND ([ChannelId] IS NULL OR [ChannelId] = @channelId)
  ORDER BY [ChannelId]
);
SELECT COALESCE(@role, 'None');
""";

    var dbResult = await _dapperService.QuerySingleAsync<string>(query, new { userId, channelId });
    if (!dbResult.IsSuccess)
    {
      _logger.LogError(dbResult.Exception, "An error occurred during execution of {MethodName} query", nameof(GetUsersOperatorRoleByChannel));
      return Result<ChatRoles>.Failure();
    }
    
    _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(GetUsersOperatorRoleByChannel), dbResult.ExecutionTime);
    return Result<ChatRoles>.Success(Enum.Parse<ChatRoles>(dbResult.Content!));
  }
}
