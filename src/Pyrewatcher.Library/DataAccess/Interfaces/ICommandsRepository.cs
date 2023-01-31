using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface ICommandsRepository
{
  Task<Command?> GetCommandForChannel(long channelId, params string[] keywords);

  Task<bool> LogCommandExecution(CommandExecution execution);
}
