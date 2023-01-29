using Pyrewatcher.Library.Riot.Enums;

namespace Pyrewatcher.Library.Riot.Utilities;

public static class RiotExtensions
{
  public static RoutingValue ToRoutingValue(this Server server)
  {
    return server switch
    {
      Server.EUNE => RoutingValue.Europe,
      Server.EUW => RoutingValue.Europe,
      _ => throw new ArgumentOutOfRangeException(nameof(server), server, "This server is unsupported")
    };
  }
}
