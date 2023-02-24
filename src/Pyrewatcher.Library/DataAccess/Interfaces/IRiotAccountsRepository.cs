using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface IRiotAccountsRepository
{
  Task<Result<IEnumerable<RiotAccount>>> GetActiveLolAccountsForApiCallsByChannelId(long channelId);
}
