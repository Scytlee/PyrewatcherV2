using Pyrewatcher.Bot.Commands.Models;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands.Interfaces;

public interface IExecutableCommand
{
  Task<CommandExecutionPartialResult> ExecuteAsync(List<string> argsList, ChatMessage message);
}
