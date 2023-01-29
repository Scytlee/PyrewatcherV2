using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pyrewatcher.Bot;
using TwitchLib.Client;

var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices((_, services) =>
               {
                 services.AddTransient<TwitchClient>();
                 services.AddTransient<BotInstance>();
                 services.AddTransient<BotInstanceManager>();
               })
               .Build();

var botInstanceManager = host.Services.GetService<BotInstanceManager>()!;

try
{
  await botInstanceManager.Initialize();
  botInstanceManager.Connect();

  await host.RunAsync();
}
catch
{
  // Log
}
finally
{
  // Log.CloseAndFlush
}