using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Models;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands;

public class AccountCommand : ICommandWithSubcommands
{
  public string Keyword { get; } = "account";
  public SemaphoreSlim Semaphore { get; } = new(1, 1);
  public Dictionary<string, ISubcommand> Subcommands { get; private set; }
  
  private readonly TwitchClient _client;
  
  public AccountCommand(TwitchClient client)
  {
    _client = client;
  }

  public void InitializeSubcommands(IEnumerable<ISubcommand> subcommands)
  {
    Subcommands = subcommands.ToDictionary(k => k.Subkeyword, v => v);
  }

  public Task<CommandExecutionPartialResult> ExecuteAsync(List<string> argsList, ChatMessage message)
  {
    _client.SendMessage(message.Channel, "This is just a test");
    return Task.FromResult(new CommandExecutionPartialResult { Result = true });
  }
}
