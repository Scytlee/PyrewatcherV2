using Pyrewatcher.Library.Models;
using Pyrewatcher.Library.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface ILolMatchesRepository
{
  Task<Result<IEnumerable<string>>> GetMatchesNotInDatabase(List<string> matches);
  Task<Result<IEnumerable<string>>> GetMatchesToUpdateByKey(string accountKey, List<string> matches);
  Task<Result<bool>> InsertMatchFromDto(string matchId, MatchV5Dto match);
  Task<Result<bool>> InsertMatchPlayerFromDto(string accountKey, string matchId, MatchParticipantV5Dto player);
}
