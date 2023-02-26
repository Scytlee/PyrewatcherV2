using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Models;
using Pyrewatcher.Bot.Interfaces;
using Pyrewatcher.Library.Models;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands;

public class AccountCommand : ICommand, ILockable
{
  public IEnumerable<string> PriorKeywords { get; } = Array.Empty<string>();
  public string Keyword { get; } = "account";
  public int Level { get; } = 1;
  public SemaphoreSlim Semaphore { get; } = new(1, 1);

  private readonly ILoggableTwitchClient _client;
  private readonly IMessageGenerator _messageGenerator;

  public AccountCommand(ILoggableTwitchClient client, IMessageGenerator messageGenerator)
  {
    _client = client;
    _messageGenerator = messageGenerator;
  }

  public async Task<CommandResult> ExecuteAsync(List<string> argsList, ChatMessage message, Channel channel)
  {
    var generatedMessage = await _messageGenerator.Generate("command_test", channel.LanguageCode);
    _client.SendMessage(message.Channel, generatedMessage);

    return CommandResult.Success;
  }
}
