using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pyrewatcher.Bot;
using Pyrewatcher.Bot.Commands;
using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Services;
using Pyrewatcher.Bot.Interfaces;
using Pyrewatcher.Bot.Serilog;
using Pyrewatcher.Bot.Services;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.DataAccess.Repositories;
using Pyrewatcher.Library.DataAccess.Services;
using Pyrewatcher.Library.Interfaces;
using Pyrewatcher.Library.Services;
using Serilog;
using Serilog.Expressions;
using Serilog.Templates;
using Serilog.Templates.Themes;
using TwitchLib.Client;
using TwitchLib.Client.Interfaces;

var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices((hostContext, services) =>
               {
                 services.AddSingleton<ITwitchClient, TwitchClient>();
                 services.AddTransient<ILoggableTwitchClient, LoggableTwitchClient>();
                 services.AddSingleton<BotInstanceManager>();
                 services.AddTransient<BotInstance>();

                 services.AddTransient<ICommandService, CommandService>();
                 services.AddSingleton<ILanguageLocalizer, LanguageLocalizer>();
                 services.AddTransient<IMessageGenerator, MessageGenerator>();

                 services.AddTransient<IDbConnectionFactory, SqlConnectionFactory>(
                   _ => new SqlConnectionFactory(hostContext.Configuration.GetConnectionString("Pyrewatcher")!));
                 services.AddTransient<IDapperService, DapperService>();
                 services.AddTransient<IChannelsRepository, ChannelsRepository>();
                 services.AddTransient<ICommandsRepository, CommandsRepository>();
                 services.AddTransient<ILanguageRepository, LanguageRepository>();
                 services.AddTransient<IOperatorsRepository, OperatorsRepository>();
                 services.AddTransient<IUsersRepository, UsersRepository>();

                 services.AddTransient<AccountCommand>();
                 services.AddTransient<AccountListSubcommand>();
                 services.AddTransient<CustomCommand>();
               })
               .UseSerilog((hostContext, _, loggerConfiguration) =>
               {
                 var myFunctions = new StaticMemberNameResolver(typeof(MyFunctions));
                 loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration)
                                    .Enrich.FromLogContext()
                                    .WriteTo.Console(new ExpressionTemplate(
                                      "[{UtcDateTime(@t):yyyy-MM-dd HH:mm:ss.fff} {@l:u4}] {Coalesce(ShortenTypeName(SourceContext), '<no source context>'),-30} | {ShortenTwitchLib(@m)}\n{@x}",
                                      theme: TemplateTheme.Literate, nameResolver: myFunctions));
               })
               .Build();

var botInstanceManager = host.Services.GetService<BotInstanceManager>()!;

try
{
  Log.Information("Application starting");
  await botInstanceManager.Initialize();
  botInstanceManager.Connect();

  await host.RunAsync();
}
catch (Exception ex)
{
  Log.Fatal(ex, "A fatal error occurred");
}
finally
{
  Log.CloseAndFlush();
}
