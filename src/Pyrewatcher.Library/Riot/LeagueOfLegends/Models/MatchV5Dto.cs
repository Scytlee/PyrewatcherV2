using Newtonsoft.Json;

namespace Pyrewatcher.Library.Riot.LeagueOfLegends.Models;

public class MatchV5Dto
{
  [JsonProperty("info")]
  public MatchInfoV5Dto Info { get; set; }
}
