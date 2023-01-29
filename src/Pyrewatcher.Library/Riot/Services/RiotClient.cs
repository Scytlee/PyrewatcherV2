using Pyrewatcher.Library.Riot.Interfaces;
using Pyrewatcher.Library.Riot.LeagueOfLegends.Interfaces;

namespace Pyrewatcher.Library.Riot.Services;

public class RiotClient : IRiotClient
{
  public IMatchV5Client MatchV5 { get; }

  public RiotClient(IMatchV5Client matchV5)
  {
    MatchV5 = matchV5;
  }
}
