using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;
using Pyrewatcher.Library.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class LolMatchesRepository : ILolMatchesRepository
{
  private readonly IDapperService _dapperService;

  public LolMatchesRepository(IDapperService dapperService)
  {
    _dapperService = dapperService;
  }

  public async Task<Result<IEnumerable<string>>> GetMatchesNotInDatabase(List<string> matches)
  {
    const string query = """
SELECT [RiotInternalId]
FROM [LeagueOfLegends].[Matches]
WHERE [RiotInternalId] IN @matches;
""";

    var result = (await _dapperService.QueryAsync<string>(query, new { matches })).Content!.ToList();

    var notInDatabase = matches.Where(x => !result.Contains(x));

    return Result<IEnumerable<string>>.Success(notInDatabase);
  }

  public async Task<Result<IEnumerable<string>>> GetMatchesToUpdateByKey(string accountKey, List<string> matches)
  {
    const string query = """
SELECT [lm].[RiotInternalId]
FROM [LeagueOfLegends].[Matches] [lm]
INNER JOIN [LeagueOfLegends].[MatchPlayers] [lmp] ON [lmp].[MatchId] = [lm].[Id]
INNER JOIN [Riot].[ChannelAccountGames] [rcag] ON [rcag].[RiotAccountGameId] = [lmp].[RiotAccountGameId]
WHERE [rcag].[Key] = @accountKey AND [lm].[RiotInternalId] IN @matches;
""";

    var dbResult = await _dapperService.QueryAsync<string>(query, new { accountKey, matches });

    var notInDatabase = matches.Where(x => !dbResult.Content!.Contains(x));

    return Result<IEnumerable<string>>.Success(notInDatabase);
  }

  public async Task<Result<bool>> InsertMatchFromDto(string matchId, MatchV5Dto match)
  {
    const string query = """
INSERT INTO [LeagueOfLegends].[Matches] ([RiotInternalId], [GameStartTimestamp], [WinningTeam], [Duration])
VALUES (@matchId, @timestamp, @winningTeam, @duration);
""";

    var dbResult = await _dapperService.ExecuteAsync(query,
      new
      {
        matchId,
        timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(match.Info.Timestamp),
        winningTeam = match.Info.Teams.First(x => x.IsWinningTeam).TeamId,
        duration = match.Info.Duration
      });

    return Result<bool>.Success(dbResult.Content == 1);
  }

  public async Task<Result<bool>> InsertMatchPlayerFromDto(string accountKey, string matchId, MatchParticipantV5Dto player)
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

    var dbResult = await _dapperService.ExecuteAsync(query, new
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

    return Result<bool>.Success(dbResult.Content == 1);
  }
}
