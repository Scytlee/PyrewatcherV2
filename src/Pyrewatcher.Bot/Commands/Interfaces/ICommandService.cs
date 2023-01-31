using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands.Interfaces;

public interface ICommandService
{
  void Initialize(BotInstance instance);

  Task HandleCommand(ChatCommand chatCommand);
}
