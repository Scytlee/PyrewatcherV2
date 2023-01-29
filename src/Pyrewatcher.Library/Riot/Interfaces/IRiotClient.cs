using Pyrewatcher.Library.Riot.LeagueOfLegends.Interfaces;

namespace Pyrewatcher.Library.Riot.Interfaces;

public interface IRiotClient
{
  public IMatchV5Client MatchV5 { get; }
}
