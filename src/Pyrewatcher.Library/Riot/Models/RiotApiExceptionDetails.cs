using Newtonsoft.Json;

namespace Pyrewatcher.Library.Riot.Models;

public class RiotApiExceptionDetails
{
  [JsonProperty("status")]
  public RiotApiExceptionDetailsStatus Status { get; set; }
}
