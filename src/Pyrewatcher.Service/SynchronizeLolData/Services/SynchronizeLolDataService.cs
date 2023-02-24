using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;
using Pyrewatcher.Library.Riot.Interfaces;
using Pyrewatcher.Library.Riot.Utilities;
using Pyrewatcher.Service.SynchronizeLolData.Interfaces;

namespace Pyrewatcher.Service.SynchronizeLolData.Services;

public class SynchronizeLolDataService : ISynchronizeLolDataService
{
  private readonly IChannelsRepository _channelsRepository;
  private readonly ILolMatchesRepository _lolMatchesRepository;
  private readonly IRiotAccountsRepository _riotAccountsRepository;

  private readonly IRiotClient _riotClient;

  public SynchronizeLolDataService(IChannelsRepository channelsRepository, ILolMatchesRepository lolMatchesRepository,
    IRiotAccountsRepository riotAccountsRepository, IRiotClient riotClient)
  {
    _channelsRepository = channelsRepository;
    _lolMatchesRepository = lolMatchesRepository;
    _riotAccountsRepository = riotAccountsRepository;
    _riotClient = riotClient;
  }

  public async Task SynchronizeLolMatchDataForActiveChannels()
  {
    var channels = (await _channelsRepository.GetConnected()).Content!.Where(channel => channel.DisplayName != "Pyrewatcher_").ToList();

    foreach (var channel in channels)
    {
      await SynchronizeLolMatchDataForChannel(channel);
    }
  }

  private async Task SynchronizeLolMatchDataForChannel(Channel channel)
  {
    var accounts = (await _riotAccountsRepository.GetActiveLolAccountsForApiCallsByChannelId(channel.Id)).Content!.ToList();

    foreach (var account in accounts)
    {
      await SynchronizeLolMatchDataForAccount(account);
    }
  }

  private async Task SynchronizeLolMatchDataForAccount(RiotAccount account)
  {
    var matches = (await _riotClient.MatchV5.GetMatchesByPuuid(account.Puuid, account.Server.ToRoutingValue(), RiotUtilities.GetStartTimeInSeconds())).ToList();

    if (!matches.Any())
    {
      return;
    }

    var matchesNotInDatabase = (await _lolMatchesRepository.GetMatchesNotInDatabase(matches)).Content!.ToList();
    var matchesNotUpdated = (await _lolMatchesRepository.GetMatchesToUpdateByKey(account.Key, matches.Except(matchesNotInDatabase).ToList())).Content!.ToList();

    var matchesToUpdate = matchesNotInDatabase.Select(match => (match, false)).Concat(matchesNotUpdated.Select(match => (match, true))).ToList();

    foreach (var (matchId, inserted) in matchesToUpdate)
    {
      var match = await _riotClient.MatchV5.GetMatchById(matchId, account.Server.ToRoutingValue());

      if (match is null)
      {
        continue;
      }

      if (match.Info.QueueId is 2000 or 2010 or 2020)
      {
        continue;
      }

      if (!match.Info.Teams.Any(x => x.IsWinningTeam))
      {
        continue;
      }

      if (!inserted)
      {
        var matchInserted = await _lolMatchesRepository.InsertMatchFromDto(matchId, match);

        if (!matchInserted.IsSuccess)
        {
          // TODO: Log failure
          continue;
        }
      }

      var player = match.Info.Players.FirstOrDefault(x => x.Puuid == account.Puuid);

      if (player is null)
      {
        // TODO: Log failure
        continue;
      }

      var playerInserted = await _lolMatchesRepository.InsertMatchPlayerFromDto(account.Key, matchId, player);

      if (!playerInserted.IsSuccess)
      {
        // TODO: Log failure
      }
    }
  }
}
