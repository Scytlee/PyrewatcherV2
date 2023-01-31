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
SELECT [StringId]
FROM [LolMatches]
WHERE [StringId] IN @matches;
""";

    using var connection = await CreateConnection();

    var result = (await connection.QueryAsync<string>(query, new { matches })).ToList();

    var notInDatabase = matches.Where(x => !result.Contains(x));

    return notInDatabase;
  }

  public async Task<IEnumerable<string>> GetMatchesToUpdateByKey(string accountKey, List<string> matches)
  {
    const string query = """
SELECT [LM].[StringId]
FROM [LolMatches] [LM]
INNER JOIN [LolMatchPlayers] [LMP] ON [LMP].[LolMatchId] = [LM].[Id]
INNER JOIN [ChannelRiotAccountGames] [CRAG] ON [CRAG].[RiotAccountGameId] = [LMP].[RiotAccountGameId]
WHERE [CRAG].[Key] = @accountKey AND [LM].[StringId] IN @matches;
""";

    using var connection = await CreateConnection();

    var result = await connection.QueryAsync<string>(query, new { accountKey, matches });

    var notInDatabase = matches.Where(x => !result.Contains(x));

    return notInDatabase;
  }

  public async Task<bool> InsertMatchFromDto(string matchId, MatchV5Dto match)
  {
    const string query = """
INSERT INTO [LolMatches] ([StringId], [GameStartTimestamp], [WinningTeam], [Duration])
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
DECLARE @LolMatchId BIGINT;
DECLARE @RiotAccountGameId BIGINT;

SELECT TOP 1 @LolMatchId = [Id]
FROM [LolMatches]
WHERE [StringId] = @matchId;

SELECT TOP 1 @RiotAccountGameId = [RiotAccountGameId]
FROM [ChannelRiotAccountGames]
WHERE [Key] = @accountKey;

INSERT INTO [LolMatchPlayers] ([LolMatchId], [RiotAccountGameId], [Team], [ChampionId], [Kills],
  [Deaths], [Assists], [ControlWardsBought])
VALUES (@LolMatchId, @RiotAccountGameId, @team, @championId, @kills, @deaths, @assists, @controlWardsBought);
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
