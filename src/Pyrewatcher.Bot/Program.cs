using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pyrewatcher.Bot;
using Pyrewatcher.Bot.Commands;
using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Services;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.DataAccess.Repositories;
using TwitchLib.Client;

var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices((_, services) =>
               {
                 services.AddSingleton<TwitchClient>();
                 services.AddSingleton<BotInstanceManager>();
                 
                 services.AddTransient<BotInstance>();
                 services.AddTransient<ICommandService, CommandService>();

                 services.AddTransient<IChannelsRepository, ChannelsRepository>();
                 services.AddTransient<ICommandAliasesRepository, CommandAliasesRepository>();
                 services.AddTransient<ICommandsRepository, CommandsRepository>();
                 services.AddTransient<IOperatorsRepository, OperatorsRepository>();
                 
                 services.AddTransient<AccountCommand>();
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