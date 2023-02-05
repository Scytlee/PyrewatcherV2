using Pyrewatcher.Library.DataAccess.Interfaces;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class CommandAliasesRepository : ICommandAliasesRepository
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly IDapperWrapper _dapperWrapper;

  public CommandAliasesRepository(IDbConnectionFactory connectionFactory, IDapperWrapper dapperWrapper)
  {
    _connectionFactory = connectionFactory;
    _dapperWrapper = dapperWrapper;
  }

  public async Task<string?> GetNewCommandByAliasAndChannelId(string alias, long channelId)
  {
    const string query = """
SELECT [NewCommand]
FROM [Command].[Aliases]
WHERE [Alias] = @alias AND ([ChannelId] IS NULL OR [ChannelId] = @channelId);
""";

    using var connection = await _connectionFactory.CreateConnection();

    var result = await _dapperWrapper.QuerySingleOrDefaultAsync<string?>(connection, query, new { alias, channelId });

    return result;
  }
}
