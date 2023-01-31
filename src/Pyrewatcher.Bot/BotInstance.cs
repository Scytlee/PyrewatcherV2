using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Library.Models;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot;

public class BotInstance
{
  public Channel Channel { get; private set; } = null!; // TODO: This could be added to constructor without breaking dependency injection

  private readonly IConfiguration _configuration;
  private readonly ILogger<BotInstance> _logger;

  private readonly ICommandService _commandService;

  public BotInstance(IConfiguration configuration, ILogger<BotInstance> logger, ICommandService commandService)
  {
    _configuration = configuration;
    _logger = logger;
    _commandService = commandService;
  }

  public void Initialize(Channel channel)
  {
    Channel = channel;
    
    _commandService.Initialize(this);
  }

  public async Task HandleCommand(ChatCommand command)
  {
    await _commandService.HandleCommand(command);
  }
}
