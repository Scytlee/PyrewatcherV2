﻿using Microsoft.Extensions.Logging;
using Pyrewatcher.Bot.Interfaces;
using TwitchLib.Client.Interfaces;

namespace Pyrewatcher.Bot.Services;

public class LoggableTwitchClient : ILoggableTwitchClient
{
  private readonly ITwitchClient _client;
  private readonly ILogger<LoggableTwitchClient> _logger;
  
  private readonly List<string> _messagesSent;
  public IEnumerable<string> MessagesSent => _messagesSent;

  public LoggableTwitchClient(ITwitchClient client, ILogger<LoggableTwitchClient> logger)
  {
    _messagesSent = new List<string>();
    _client = client;
    _logger = logger;
  }

  public void SendMessage(string? channel, string? message, bool dryRun = false)
  {
    if (channel is null || message is null)
    {
      return;
    }
    
    _client.SendMessage(channel: channel, message: message, dryRun: dryRun);
    _messagesSent.Add(message);
    
    if (!dryRun)
    {
      _logger.LogInformation("Message sent to channel {Channel}: {Message}", channel, message);
    }
  }
}
