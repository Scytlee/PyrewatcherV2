using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Enums;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class OperatorsRepository : RepositoryBase, IOperatorsRepository
{
  public OperatorsRepository(IConfiguration configuration) : base(configuration)
  {
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

    using var connection = await CreateConnection();

    var result = await connection.QuerySingleAsync<string>(query, new { userId, channelId });

    return Enum.Parse<ChatRoles>(result);
  }
}
