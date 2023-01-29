namespace Pyrewatcher.Bot.Commands.Models;

public class ConfigurationCommand
{
  public string Keyword { get; set; }
  public bool Enabled { get; set; }
  public int CooldownInSeconds { get; set; }
  
  public IEnumerable<ConfigurationSubcommand> Subcommands { get; set; }
}