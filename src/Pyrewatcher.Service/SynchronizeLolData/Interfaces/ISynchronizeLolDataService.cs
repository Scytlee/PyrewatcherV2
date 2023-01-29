using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Service.SynchronizeLolData.Interfaces;

public interface ISynchronizeLolDataService
{
  Task SynchronizeLolMatchDataForActiveChannels();
}
