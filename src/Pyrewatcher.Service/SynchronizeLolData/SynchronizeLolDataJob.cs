using Pyrewatcher.Service.SynchronizeLolData.Interfaces;
using Quartz;

namespace Pyrewatcher.Service.SynchronizeLolData;

public class SynchronizeLolDataJob : IJob
{
  private readonly ISynchronizeLolDataService _service;

  public SynchronizeLolDataJob(ISynchronizeLolDataService service)
  {
    _service = service;
  }

  public async Task Execute(IJobExecutionContext context)
  {
    await _service.SynchronizeLolMatchDataForActiveChannels();
  }
}
