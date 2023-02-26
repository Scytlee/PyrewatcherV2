using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IUsersRepository
{
  Task<Result<None>> UpsertUser(long id, string displayName);
}
