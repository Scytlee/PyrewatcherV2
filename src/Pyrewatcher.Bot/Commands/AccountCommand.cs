using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Models;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands;

public class AccountCommand : ICommand, ILockable
{
  public IEnumerable<string> PriorKeywords { get; } = Array.Empty<string>();
  public string Keyword { get; } = "account";
  public int Level { get; } = 1;
  public SemaphoreSlim Semaphore { get; } = new(1, 1);

  private readonly ILoggableTwitchClient _client;

  public AccountCommand(ILoggableTwitchClient client)
  {
    _client = client;
  }

  public Task<ExecutionResult> ExecuteAsync(List<string> argsList, ChatMessage message)
  {
    _client.SendMessage(message.Channel, "This is just a test");
    return Task.FromResult(new ExecutionResult { Result = true });
  }
}
