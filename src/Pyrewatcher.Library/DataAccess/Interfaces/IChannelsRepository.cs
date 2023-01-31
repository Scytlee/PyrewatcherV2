using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IChannelsRepository
{
  Task<IEnumerable<Channel>> GetConnected();
}
