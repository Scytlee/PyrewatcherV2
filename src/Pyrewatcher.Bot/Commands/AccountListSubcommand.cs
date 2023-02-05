﻿using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Models;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands;

public class AccountListSubcommand : ICommand
{
  public string[] PriorKeywords { get; } = { "account" };
  public string Keyword { get; } = "list";
  public int Level { get; } = 2;

  private readonly ITwitchClient _client;

  public AccountListSubcommand(ITwitchClient client)
  {
    _client = client;
  }

  public Task<ExecutionResult> ExecuteAsync(List<string> argsList, ChatMessage message)
  {
    _client.SendMessage(message.Channel, "This is just another test");
    return Task.FromResult(new ExecutionResult { Result = true });
  }
}