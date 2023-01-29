using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;
using Pyrewatcher.Library.Riot.Enums;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class RiotAccountsRepository : RepositoryBase, IRiotAccountsRepository
{
  public RiotAccountsRepository(IConfiguration config) : base(config)
  {
  }

  public async Task<IEnumerable<RiotAccount>> GetActiveLolAccountsForApiCallsByChannelId(long channelId)
  {
    const string query = """
SELECT [CRAG].[Key], [RA].[SummonerName], [RA].[Server],
  [CRAG].[DisplayName], [RAG].[SummonerId], [RAG].[AccountId], [RAG].[Puuid],
  [RAG].[Tier], [RAG].[Rank], [RAG].[LeaguePoints], [RAG].[SeriesProgress]
FROM [new].[ChannelRiotAccountGames] [CRAG]
INNER JOIN [new].[RiotAccountGames] [RAG] ON [RAG].[Id] = [CRAG].[RiotAccountGameId]
INNER JOIN [new].[RiotAccounts] [RA] ON [RA].[Id] = [RAG].[RiotAccountId]
WHERE [CRAG].[ChannelId] = @channelId AND [CRAG].[Active] = 1 AND [RAG].[Game] = @game;
""";

    using var connection = await CreateConnection();

    var result = (await connection.QueryAsync<RiotAccount>(query, new { channelId, game = Game.LeagueOfLegends })).ToList();

    return result;
  }
}
