using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Models;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands;

public class AccountListSubcommand : ICommand
{
  public IEnumerable<string> PriorKeywords { get; } = new[] { "account" };
  public string Keyword { get; } = "list";
  public int Level { get; } = 2;

  private readonly ILoggableTwitchClient _client;

  public AccountListSubcommand(ILoggableTwitchClient client)
  {
    _client = client;
  }

  public Task<CommandResult> ExecuteAsync(List<string> argsList, ChatMessage message)
  {
    _client.SendMessage(message.Channel, "This is just another test");
    return Task.FromResult(CommandResult.Success);
  }
}
