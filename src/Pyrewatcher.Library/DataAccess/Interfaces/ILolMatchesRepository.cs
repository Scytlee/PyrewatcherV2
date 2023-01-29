using Pyrewatcher.Library.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface ILolMatchesRepository
{
  Task<IEnumerable<string>> GetMatchesNotInDatabase(List<string> matches);
  Task<IEnumerable<string>> GetMatchesToUpdateByKey(string accountKey, List<string> matches);
  Task<bool> InsertMatchFromDto(string matchId, MatchV5Dto match);
  Task<bool> InsertMatchPlayerFromDto(string accountKey, string matchId, MatchParticipantV5Dto player);
}
