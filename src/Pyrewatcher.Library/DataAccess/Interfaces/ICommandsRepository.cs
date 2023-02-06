using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface ICommandsRepository
{
  Task<Command?> GetCommandForChannel(long channelId, IEnumerable<string> commandAsList);

  Task<bool> LogCommandExecution(CommandExecution execution);
}
