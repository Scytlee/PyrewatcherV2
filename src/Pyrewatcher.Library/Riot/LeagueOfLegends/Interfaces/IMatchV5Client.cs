using Pyrewatcher.Library.Riot.Enums;
using Pyrewatcher.Library.Riot.LeagueOfLegends.Models;

namespace Pyrewatcher.Library.Riot.LeagueOfLegends.Interfaces;

public interface IMatchV5Client
{
  Task<IEnumerable<string>> GetMatchesByPuuid(string puuid, RoutingValue routingValue, long? startTime = null, long? endTime = null, int? queue = null,
    string type = null, int? start = null, int? count = null);

  Task<MatchV5Dto> GetMatchById(string matchId, RoutingValue routingValue);
}
