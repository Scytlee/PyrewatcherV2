using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;
using Pyrewatcher.Library.Riot.Enums;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class RiotAccountsRepository : IRiotAccountsRepository
{
  private readonly IDbConnectionFactory _connectionFactory;
  private readonly IDapperWrapper _dapperWrapper;

  public RiotAccountsRepository(IDbConnectionFactory connectionFactory, IDapperWrapper dapperWrapper)
  {
    _connectionFactory = connectionFactory;
    _dapperWrapper = dapperWrapper;
  }

  public async Task<IEnumerable<RiotAccount>> GetActiveLolAccountsForApiCallsByChannelId(long channelId)
  {
    const string query = """
SELECT [rcag].[Key], [ra].[SummonerName], [ra].[Server],
  [rcag].[DisplayName], [rag].[SummonerId], [rag].[AccountId], [rag].[Puuid],
  [rag].[Tier], [rag].[Rank], [rag].[LeaguePoints], [rag].[SeriesProgress]
FROM [Riot].[ChannelAccountGames] [rcag]
INNER JOIN [Riot].[AccountGames] [rag] ON [rag].[Id] = [rcag].[RiotAccountGameId]
INNER JOIN [Riot].[Accounts] [ra] ON [ra].[Id] = [rag].[RiotAccountId]
WHERE [rcag].[ChannelId] = @channelId AND [rcag].[Active] = 1 AND [rag].[Game] = @game;
""";

    using var connection = await _connectionFactory.CreateConnection();

    var result = (await _dapperWrapper.QueryAsync<RiotAccount>(connection, query, new { channelId, game = Game.LeagueOfLegends })).ToList();

    return result;
  }
}
