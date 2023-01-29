using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IBroadcastersRepository
{
  Task<IEnumerable<Channel>> GetConnected();
}
