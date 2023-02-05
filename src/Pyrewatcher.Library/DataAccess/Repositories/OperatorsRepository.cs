﻿using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Enums;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class OperatorsRepository : IOperatorsRepository
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly IDapperWrapper _dapperWrapper;

  public OperatorsRepository(IDbConnectionFactory connectionFactory, IDapperWrapper dapperWrapper)
  {
    _connectionFactory = connectionFactory;
    _dapperWrapper = dapperWrapper;
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

    var result = await _dapperWrapper.QuerySingleAsync<string>(connection, query, new { userId, channelId });

    return Enum.Parse<ChatRoles>(result);
  }
}
