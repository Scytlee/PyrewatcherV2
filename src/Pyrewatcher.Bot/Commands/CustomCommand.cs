using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Models;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands;

public class CustomCommand : ICommand, ILockable
{
  public IEnumerable<string> PriorKeywords { get; } = Array.Empty<string>();
  public string Keyword { get; } = string.Empty;
  public int Level { get; } = 1;
  public SemaphoreSlim Semaphore { get; } = new(1, 1);
  
  private readonly ITwitchClient _client;
  
  public CustomCommand(ITwitchClient client)
  {
    _client = client;
  }
  
  public Task<ExecutionResult> ExecuteAsync(List<string> argsList, ChatMessage message)
  {
    var customText = argsList.First();
    _client.SendMessage(message.Channel, customText);
    return Task.FromResult(new ExecutionResult { Result = true });
  }
}
