namespace Pyrewatcher.Bot.Interfaces;

public interface ILoggableTwitchClient
{
  public IEnumerable<string> MessagesSent { get; }

  void SendMessage(string? channel, string? message, bool dryRun = false);
}
