using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface ICommandAliasesRepository
{
  Task<Result<string?>> GetNewCommandByAliasAndChannelId(string alias, long channelId);
}
