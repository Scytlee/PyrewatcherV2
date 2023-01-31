namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface ICommandAliasesRepository
{
  Task<string?> GetNewCommandByAliasAndChannelId(string alias, long channelId);
}
