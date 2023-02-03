using Pyrewatcher.Bot.Commands.Models;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands.Interfaces;

public interface ICommand
{
  string Keyword { get; }
  
  Task<ExecutionResult> ExecuteAsync(List<string> argsList, ChatMessage message);
}
