using Pyrewatcher.Library.Enums;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IOperatorsRepository
{
  Task<Result<ChatRoles>> GetUsersOperatorRoleByChannel(long userId, long channelId);
}
