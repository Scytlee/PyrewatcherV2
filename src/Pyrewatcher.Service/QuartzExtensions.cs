using Microsoft.Extensions.Configuration;
using Quartz;

namespace Pyrewatcher.Service;

public static class QuartzExtensions
{
  public static void ScheduleJob<T>(this IServiceCollectionQuartzConfigurator quartz, IConfiguration config) where T : IJob
  {
    var jobName = typeof(T).Name;
    var section = config.GetSection($"Jobs:{jobName}");

    var enabled = bool.Parse(section["Enabled"]);
    if (!enabled)
    {
      return;
    }

    var minutesInterval = int.Parse(section["IntervalInMinutes"]);

    var jobKey = new JobKey(jobName);
    quartz.AddJob<T>(options => options.WithIdentity(jobKey));

    quartz.AddTrigger(options =>
    {
      options.ForJob(jobKey);
      options.WithIdentity($"{jobName} trigger");
      options.StartNow();
      options.WithSimpleSchedule(schedule =>
      {
        schedule.WithIntervalInMinutes(minutesInterval);
        schedule.RepeatForever();
      });
    });
  }
}
