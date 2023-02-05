using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Enums;
using System.Diagnostics;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class OperatorsRepository : IOperatorsRepository
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly IDapperWrapper _dapperWrapper;
  private readonly ILogger<OperatorsRepository> _logger;

  public OperatorsRepository(IDbConnectionFactory connectionFactory, IDapperWrapper dapperWrapper, ILogger<OperatorsRepository> logger)
  {
    _connectionFactory = connectionFactory;
    _dapperWrapper = dapperWrapper;
    _logger = logger;
  }

  public async Task<ChatRoles> GetUsersOperatorRoleByChannel(long userId, long channelId)
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

    using var connection = await _connectionFactory.CreateConnection();
    
    var stopwatch = Stopwatch.StartNew();
    try
    {
      var result = await _dapperWrapper.QuerySingleAsync<string>(connection, query, new { userId, channelId });
      stopwatch.Stop();
      _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(GetUsersOperatorRoleByChannel), stopwatch.ElapsedMilliseconds);
      return Enum.Parse<ChatRoles>(result);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "An error occurred during execution of {MethodName} query", nameof(GetUsersOperatorRoleByChannel));
      return ChatRoles.None;
    }

  }
}
