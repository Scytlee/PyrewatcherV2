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

  public async Task<ChatRoles?> GetUsersOperatorRoleByChannel(long userId, long channelId)
  {
    // TODO: Improve the query to return the string enum
    // if query returns 0, then user is a global operator
    // if query returns channelId, then user is a channel operator
    // if query returns null, then user is not an operator
    // query cannot return any other value than these 3
    const string query = """
SELECT TOP 1 COALESCE([ChannelId], 0)
FROM [new].[Operators]
WHERE [UserId] = @userId AND ([ChannelId] IS NULL OR [ChannelId] = @channelId)
ORDER BY [ChannelId];
""";

    using var connection = await CreateConnection();

    var result = await connection.QuerySingleOrDefaultAsync<long?>(query, new { userId, channelId });

    return result switch
    {
      null => null,
      0 => ChatRoles.GlobalOperator,
      _ => ChatRoles.ChannelOperator
    };
  }
}
