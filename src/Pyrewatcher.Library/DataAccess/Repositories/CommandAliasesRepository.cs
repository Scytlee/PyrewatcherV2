using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Library.DataAccess.Interfaces;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class CommandAliasesRepository : RepositoryBase, ICommandAliasesRepository
{
  public CommandAliasesRepository(IConfiguration configuration) : base(configuration)
  {
  }

  public async Task<string?> GetNewCommandByAliasAndChannelId(string alias, long channelId)
  {
    const string query = """
SELECT [NewCommand]
FROM [new].[CommandAliases]
WHERE [Alias] = @alias AND ([ChannelId] IS NULL OR [ChannelId] = @channelId);
""";

    using var connection = await CreateConnection();

    var result = await connection.QuerySingleOrDefaultAsync<string?>(query, new { alias, channelId });

    return result;
  }
}
