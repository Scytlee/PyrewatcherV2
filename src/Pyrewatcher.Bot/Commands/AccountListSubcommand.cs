using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Models;
using Pyrewatcher.Bot.Interfaces;
using Pyrewatcher.Library.Models;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands;

public class AccountListSubcommand : ICommand
{
  public IEnumerable<string> PriorKeywords { get; } = new[] { "account" };
  public string Keyword { get; } = "list";
  public int Level { get; } = 2;

  private readonly ILoggableTwitchClient _client;
  private readonly IMessageGenerator _messageGenerator;

  public AccountListSubcommand(ILoggableTwitchClient client, IMessageGenerator messageGenerator)
  {
    _client = client;
    _messageGenerator = messageGenerator;
  }

  public async Task<CommandResult> ExecuteAsync(List<string> argsList, ChatMessage message, Channel channel)
  {
    var generatedMessage = await _messageGenerator.Generate("command_test_formatted", channel.LanguageCode,
      parameters: new { Fruit = "Apple", Vegetable = "Carrot" }, mention: message.DisplayName);
    _client.SendMessage(message.Channel, generatedMessage);

    return CommandResult.Success;
  }
}
