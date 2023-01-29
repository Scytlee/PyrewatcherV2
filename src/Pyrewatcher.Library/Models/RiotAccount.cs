using Pyrewatcher.Library.Riot.Enums;

namespace Pyrewatcher.Library.Models;

public class RiotAccount
{
  // from RiotAccounts
  public Server Server { get; set; }
  public string SummonerName { get; set; }
  
  // from RiotAccountGames
  public Game Game { get; set; }
  public string? SummonerId { get; set; }
  public string? AccountId { get; set; }
  public string? Puuid { get; set; }
  public string? Tier { get; set; }
  public string? Rank { get; set; }
  public string? LeaguePoints { get; set; }
  public string? SeriesProgress { get; set; }
  
  // from ChannelRiotAccountGames
  public string Key { get; set; } = string.Empty;
  public string DisplayName { get; set; } = string.Empty;
  public bool Active { get; set; } // = false;

  public string? DisplayableRank
  {
    get
    {
      if (Tier is null)
      {
        return null;
      }
  
      var output = Tier is "MASTER" or "GRANDMASTER" or "CHALLENGER" ? $"{Tier} {LeaguePoints} LP" : $"{Tier} {Rank} {LeaguePoints} LP";
  
      if (SeriesProgress != null)
      {
        output += $" ({SeriesProgress.Replace('N', '-').Replace('W', '✔').Replace('L', '✖')})";
      }
  
      return output;
    }
  }

  public override string ToString() => $"[{Key}] ➔ {DisplayName}";
}