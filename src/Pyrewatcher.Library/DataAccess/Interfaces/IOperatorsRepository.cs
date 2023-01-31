using Pyrewatcher.Library.Enums;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IOperatorsRepository
{
  Task<ChatRoles?> GetUsersOperatorRoleByChannel(long userId, long channelId);
}
