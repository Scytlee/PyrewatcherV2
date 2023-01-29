using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IRiotAccountsRepository
{
  Task<IEnumerable<RiotAccount>> GetActiveLolAccountsForApiCallsByChannelId(long channelId);
}
