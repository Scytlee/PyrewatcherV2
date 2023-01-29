using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.DataAccess.Repositories;
using Pyrewatcher.Service;
using Pyrewatcher.Service.SynchronizeLolData;
using Pyrewatcher.Service.SynchronizeLolData.Interfaces;
using Pyrewatcher.Service.SynchronizeLolData.Services;
using Quartz;

var host = Host.CreateDefaultBuilder()
               .ConfigureServices((hostContext, services) =>
               {
                 services.AddTransient<ISynchronizeLolDataService, SynchronizeLolDataService>();
                 services.AddTransient<IBroadcastersRepository, BroadcastersRepository>();
                 services.AddTransient<ILolMatchesRepository, LolMatchesRepository>();
                 services.AddTransient<IRiotAccountsRepository, RiotAccountsRepository>();
                 
                 services.AddQuartz(q =>
                 {
                   q.UseMicrosoftDependencyInjectionJobFactory();
                   q.ScheduleJob<SynchronizeLolDataJob>(hostContext.Configuration);
                 });
                 services.AddQuartzHostedService(options =>
                 {
                   options.WaitForJobsToComplete = true;
                 });
               })
               .Build();

await host.RunAsync();
