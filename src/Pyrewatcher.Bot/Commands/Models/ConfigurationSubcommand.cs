namespace Pyrewatcher.Bot.Commands.Models;

public class ConfigurationSubcommand
{
  public string Keyword { get; set; }
  public bool Enabled { get; set; }
  public int CooldownInSeconds { get; set; }
}
