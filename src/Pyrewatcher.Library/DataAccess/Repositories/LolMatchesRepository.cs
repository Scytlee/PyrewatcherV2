using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class LolMatchesRepository : RepositoryBase, ILolMatchesRepository
{
  public LolMatchesRepository(IConfiguration configuration) : base(configuration)
  {
  }

  public async Task<IEnumerable<string>> GetMatchesNotInDatabase(List<string> matches)
  {
    const string query = """
SELECT [RiotInternalId]
FROM [LeagueOfLegends].[Matches]
WHERE [RiotInternalId] IN @matches;
""";

    using var connection = await CreateConnection();

    var result = (await connection.QueryAsync<string>(query, new { matches })).ToList();

    var notInDatabase = matches.Where(x => !result.Contains(x));

    return notInDatabase;
  }

  public async Task<IEnumerable<string>> GetMatchesToUpdateByKey(string accountKey, List<string> matches)
  {
    const string query = """
SELECT [lm].[RiotInternalId]
FROM [LeagueOfLegends].[Matches] [lm]
INNER JOIN [LeagueOfLegends].[MatchPlayers] [lmp] ON [lmp].[MatchId] = [lm].[Id]
INNER JOIN [Riot].[ChannelAccountGames] [rcag] ON [rcag].[RiotAccountGameId] = [lmp].[RiotAccountGameId]
WHERE [rcag].[Key] = @accountKey AND [lm].[RiotInternalId] IN @matches;
""";

    using var connection = await CreateConnection();

    var result = await connection.QueryAsync<string>(query, new { accountKey, matches });

    var notInDatabase = matches.Where(x => !result.Contains(x));

    return notInDatabase;
  }

  public async Task<bool> InsertMatchFromDto(string matchId, MatchV5Dto match)
  {
    const string query = """
INSERT INTO [LeagueOfLegends].[Matches] ([RiotInternalId], [GameStartTimestamp], [WinningTeam], [Duration])
VALUES (@matchId, @timestamp, @winningTeam, @duration);
""";

    using var connection = await CreateConnection();

    var rows = await connection.ExecuteAsync(query, new
    {
      matchId,
      timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(match.Info.Timestamp),
      winningTeam = match.Info.Teams.First(x => x.IsWinningTeam).TeamId,
      duration = match.Info.Duration
    });

    return rows == 1;
  }

  public async Task<bool> InsertMatchPlayerFromDto(string accountKey, string matchId, MatchParticipantV5Dto player)
  {
    const string query = """
DECLARE @lolMatchId BIGINT = (
  SELECT TOP 1 [Id]
  FROM [LeagueOfLegends].[Matches]
  WHERE [RiotInternalId] = @matchId
);

DECLARE @riotAccountGameId BIGINT = (
  SELECT TOP 1 [RiotAccountGameId]
  FROM [Riot].[ChannelAccountGames]
  WHERE [Key] = @accountKey
);

INSERT INTO [LeagueOfLegends].[MatchPlayers] ([MatchId], [RiotAccountGameId], [Team], [ChampionId], [Kills],
  [Deaths], [Assists], [ControlWardsBought])
VALUES (@lolMatchId, @riotAccountGameId, @team, @championId, @kills, @deaths, @assists, @controlWardsBought);
""";

    using var connection = await CreateConnection();

    var rows = await connection.ExecuteAsync(query, new
    {
      matchId,
      accountKey,
      team = player.Team,
      championId = player.ChampionId,
      kills = player.Kills,
      deaths = player.Deaths,
      assists = player.Assists,
      controlWardsBought = player.VisionWardsBought
    });

    return rows == 1;
  }
}
