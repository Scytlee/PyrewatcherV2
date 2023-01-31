namespace Pyrewatcher.Library.Models;

public class Channel
{
  public long Id { get; set; }
  public bool Connected { get; set; }// = false;
  public string LanguageCode { get; set; } = "EN";
  public string ResetTimeUtc { get; set; } = "06:00";
  public bool SubGreetingsEnabled { get; set; }// = false;
  public string SubGreeting { get; set; } = "@{0} HeyGuys <3";

  public string DisplayName { get; set; } = string.Empty;
  public string NormalizedName { get; set; } = string.Empty;
}
