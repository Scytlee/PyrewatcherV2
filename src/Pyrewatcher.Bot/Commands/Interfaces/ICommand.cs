using Pyrewatcher.Bot.Commands.Models;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands.Interfaces;

public interface ICommand
{
  IEnumerable<string> PriorKeywords { get; }
  string Keyword { get; }
  int Level { get; }
  
  Task<CommandResult> ExecuteAsync(List<string> argsList, ChatMessage message);
}
