using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Bot;

public class BotInstance
{
  public Channel Channel { get; private set; } = null!; // TODO: This could be added to constructor without breaking dependency injection
  
  private readonly IConfiguration _configuration;
  private readonly ILogger<BotInstance> _logger;

  // private readonly CommandService _commandService;
  
  public BotInstance(IConfiguration configuration, ILogger<BotInstance> logger)
  {
    _configuration = configuration;
    _logger = logger;
  }

  public void Initialize(Channel channel)
  {
    Channel = channel;
  }
}
