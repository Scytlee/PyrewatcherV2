namespace Pyrewatcher.Library.Models;

public class CommandOverride
{
  public string Keywords { get; set; }
  public bool? Enabled { get; set; }
  public int? CooldownInSeconds { get; set; }
  public int? Permissions { get; set; }
}
