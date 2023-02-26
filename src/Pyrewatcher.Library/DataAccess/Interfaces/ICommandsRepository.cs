using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface ICommandsRepository
{
  Task<Result<string?>> GetNewCommandByAliasAndChannelId(string alias, long channelId);

  Task<Result<Command?>> GetCommandForChannel(long channelId, IEnumerable<string> commandAsList);

  Task<Result<bool>> LogCommandExecution(CommandExecution execution);
}
