﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.Bot.Commands.Interfaces;
using Pyrewatcher.Bot.Commands.Models;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Enums;
using Pyrewatcher.Library.Models;
using Pyrewatcher.Library.Utilities;
using System.Diagnostics;
using System.Reflection;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Bot.Commands.Services;

public class CommandService : ICommandService
{
  private BotInstance _instance = null!; // TODO: This could be added to constructor without breaking dependency injection

  private readonly List<CommandTree> _commandForest;
  private readonly CustomCommand _customCommand;

  private readonly IConfiguration _configuration;
  private readonly ILogger<CommandService> _logger;

  private readonly ICommandsRepository _commandsRepository;
  private readonly IOperatorsRepository _operatorsRepository;
  private readonly IUsersRepository _usersRepository;

  public CommandService(IServiceProvider serviceProvider, CustomCommand customCommand, IConfiguration configuration, ILogger<CommandService> logger,
    ICommandsRepository commandsRepository, IOperatorsRepository operatorsRepository, IUsersRepository usersRepository)
  {
    _customCommand = customCommand;
    _configuration = configuration;
    _logger = logger;
    _commandsRepository = commandsRepository;
    _operatorsRepository = operatorsRepository;
    _usersRepository = usersRepository;

    _commandForest = GetCommandForest(serviceProvider);
  }

  public void Initialize(BotInstance instance)
  {
    _instance = instance;
  }

  public List<CommandTree> GetCommandForest(IServiceProvider serviceProvider)
  {
    var commands = Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Where(type => type.IsClass)
                           .Where(type => type.IsAssignableTo(typeof(ICommand)))
                           .Select(type => (ICommand?) serviceProvider.GetService(type)!)
                           .GroupBy(command => command.PriorKeywords.FirstOrDefault() ?? command.Keyword)
                           .ToList();

    var commandForest = new List<CommandTree>();

    foreach (var grouping in commands)
    {
      var commandsOrdered = grouping.OrderBy(command => command.Level).ToList();
      var tree = new CommandTree(commandsOrdered.First());
      foreach (var command in commandsOrdered.Skip(1))
      {
        var parentNode = tree.FindChild(node => command.PriorKeywords.SequenceEqual(node.Value.PriorKeywords.Concat(new[] { node.Value.Keyword })));
        parentNode?.AddChild(command);
      }

      commandForest.Add(tree);
    }

    return commandForest;
  }

  public async Task HandleCommand(ChatCommand chatCommand)
  {
    var chatMessage = chatCommand.ChatMessage;
    _logger.LogInformation("{User} issued command \"{Command}\" in channel {Channel}", chatMessage.DisplayName, chatMessage.Message, chatMessage.Channel);
    var timestampUtc = DateTime.UtcNow;
    var stopwatch = Stopwatch.StartNew();

    await _usersRepository.UpsertUser(long.Parse(chatMessage.UserId), chatMessage.DisplayName);

    // 1. Check for an alias
    var aliasResult = await _commandsRepository.GetNewCommandByAliasAndChannelId($"{chatCommand.CommandIdentifier}{chatCommand.CommandText}",
      _instance.Channel.Id);
    if (!aliasResult.IsSuccess)
    {
      return;
    }
    // If command identifier is '!', then it's supported only if it's an alias
    if (aliasResult.Content is null && chatCommand.CommandIdentifier == '!')
    {
      _logger.LogInformation("Command starts with '!' and its alias was not found - returning");
      return;
    }

    var fullCommandAsList = (aliasResult.Content ?? chatCommand.CommandText).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                                                            .Concat(chatCommand.ArgumentsAsList)
                                                                            .ToList();

    _logger.LogDebug("Command being executed: \"\\{Command}\"", string.Join(' ', fullCommandAsList));

    // Check if command is available for the channel
    var commandResult = await _commandsRepository.GetCommandForChannel(_instance.Channel.Id, fullCommandAsList);
    if (!commandResult.IsSuccess)
    {
      return;
    }
    if (commandResult.Content is null)
    {
      _logger.LogInformation("Command was not found - returning");
      return;
    }

    _logger.LogDebug("Command found: \"\\{FullCommand}\", type: {Type}", commandResult.Content.FullCommand, commandResult.Content.Type.ToString());

    // Find command
    ICommand? rootCommand = null;
    ICommand? commandToExecute = null;

    switch (commandResult.Content.Type)
    {
      case CommandType.Stock:
        // Traverse the command forest
        foreach (var tree in _commandForest)
        {
          var node = tree.FindChild(node =>
            node.Value.PriorKeywords.SequenceEqual(commandResult.Content.PriorKeywords) && node.Value.Keyword == commandResult.Content.Keyword);
          if (node is not null)
          {
            rootCommand = node.Root.Value;
            commandToExecute = node.Value;
            break;
          }
        }
        break;
      case CommandType.Custom:
        rootCommand = _customCommand;
        commandToExecute = _customCommand;
        break;
      default:
        rootCommand = null;
        commandToExecute = null;
        break;
    }

    if (rootCommand is null || commandToExecute is null)
    {
      _logger.LogWarning("Class instance for command \"\\{FullCommand}\" does not exist - returning", commandResult.Content.FullCommand);
      return;
    }

    _logger.LogDebug("Class instance for command \"\\{FullCommand}\" found: {TypeName}", commandResult.Content.FullCommand, commandToExecute.GetType().Name);

    // Prevent concurrent command executions
    // https://stackoverflow.com/questions/20084695/c-sharp-lock-and-async-method
    var semaphore = rootCommand is ILockable lockable ? lockable.Semaphore : null; // main command should always be lockable

    if (semaphore is not null)
    {
      await semaphore.WaitAsync();
      _logger.LogDebug("Semaphore of command \"\\{Command}\" for channel {Channel} has been claimed", commandResult.Content.FirstKeyword, chatMessage.Channel);
    }

    try
    {
      // Attempt to execute command
      var commandExecutionResult = await ExecuteCommand(commandResult.Content, commandToExecute, chatMessage, fullCommandAsList);

      stopwatch.Stop();
      if (commandExecutionResult.IsSuccess)
      {
        _logger.LogInformation("Command \"\\{Command}\" has been successfully executed in {Time} ms", string.Join(' ', fullCommandAsList),
          stopwatch.ElapsedMilliseconds);
      }
      else
      {
        _logger.LogInformation("Command \"\\{Command}\" has executed without success in {Time} ms", string.Join(' ', fullCommandAsList),
          stopwatch.ElapsedMilliseconds);
      }

      var resultToLog = new CommandExecution
      {
        ChannelId = _instance.Channel.Id,
        UserId = long.Parse(chatMessage.UserId),
        CommandId = commandResult.Content.Id,
        InputCommand = $"{chatCommand.CommandIdentifier}{chatCommand.CommandText} {chatCommand.ArgumentsAsString}",
        ExecutedCommand = $"\\{string.Join(' ', fullCommandAsList)}",
        Result = commandExecutionResult.IsSuccess,
        TimestampUtc = timestampUtc,
        TimeInMilliseconds = stopwatch.ElapsedMilliseconds,
        Comment = commandExecutionResult.Comment
      };

      await _commandsRepository.LogCommandExecution(resultToLog);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "An error occurred during command execution");
    }
    finally
    {
      if (semaphore is not null)
      {
        semaphore.Release();
        _logger.LogDebug("Semaphore of command \"\\{Command}\" for channel {Channel} has been released", commandResult.Content.FirstKeyword,
          chatMessage.Channel);
      }
    }
  }

  private async Task<CommandResult> ExecuteCommand(Command commandInfo, ICommand commandInstance, ChatMessage chatMessage, List<string> fullCommandAsList)
  {
    // Check if user is permitted to use the command
    var userRoles = await GetUserRoles(chatMessage);
    if (!IsUserPermitted(commandInfo.Permissions, userRoles))
    {
      _logger.LogInformation("User {User} has insufficient permissions to execute command \"\\{Command}\"", chatMessage.DisplayName, commandInfo.FullCommand);
      _logger.LogDebug("User permissions: {UserPermissions}. Command requirement: {CommandRequirement}", userRoles, commandInfo.Permissions);
      return CommandResult.Failure($"Insufficient permissions (command required {commandInfo.Permissions}, user had {userRoles})");
    }

    // Check if user is an operator
    bool isUserOperator;
    if (userRoles.HasFlag(ChatRoles.GlobalOperator))
    {
      isUserOperator = true;
      _logger.LogDebug("User {User} is a global operator", chatMessage.DisplayName);
    }
    else if (userRoles.HasFlag(ChatRoles.ChannelOperator))
    {
      isUserOperator = true;
      _logger.LogDebug("User {User} is an operator of channel {Channel}", chatMessage.DisplayName, chatMessage.Channel);
    }
    else
    {
      isUserOperator = false;
      _logger.LogDebug("User {User} is not an operator", chatMessage.DisplayName);
    }

    // Check if command is on cooldown
    // Cooldown for operators is no longer than 2 seconds
    var userCooldown = TimeSpan.FromSeconds(isUserOperator ? Math.Min(commandInfo.CooldownInSeconds, 2) : commandInfo.CooldownInSeconds);

    if (commandInfo.LatestExecutionUtc is not null)
    {
      var lastUsage = DateTime.UtcNow - commandInfo.LatestExecutionUtc.Value;

      if (lastUsage < userCooldown)
      {
        var secondsLeft = (userCooldown - lastUsage).TotalMilliseconds / 1000.0;
        _logger.LogInformation("Command \"\\{Command}\" is on cooldown - {SecondsLeft:F2}s left", commandInfo.FirstKeyword, secondsLeft);
        return CommandResult.Failure($"Command on cooldown - {secondsLeft:F2}s left{(isUserOperator ? " (operator)" : string.Empty)}");
      }
    }
    else
    {
      _logger.LogDebug("First execution of command \"\\{Command}\" on channel {Channel}", string.Join(' ', fullCommandAsList), chatMessage.Channel);
    }

    // Execute command
    return await commandInstance.ExecuteAsync(
      commandInfo.CustomText is not null ? new List<string> { commandInfo.CustomText } : fullCommandAsList.Skip(commandInstance.Level).ToList(), chatMessage,
      _instance.Channel);
  }

  public async Task<ChatRoles> GetUserRoles(ChatMessage message)
  {
    // Ban/restriction check
    // This is something that might be implemented in the future

    var userRoles = ChatRoles.Viewer;

    // Database operator check
    var operatorCheckResult = await _operatorsRepository.GetUsersOperatorRoleByChannel(long.Parse(message.UserId), _instance.Channel.Id);
    userRoles |= operatorCheckResult.Content;

    // Trusted check
    // This is something that might be implemented in the future

    // Twitch hierarchy checks
    if (message.IsBroadcaster)
    {
      userRoles |= ChatRoles.Broadcaster | ChatRoles.Moderator | ChatRoles.Vip;
    }
    else if (message.IsModerator)
    {
      userRoles |= ChatRoles.Moderator | ChatRoles.Vip;
    }
    else if (message.IsVip)
    {
      userRoles |= ChatRoles.Vip;
    }

    // Subscription check
    if (message.IsSubscriber)
    {
      userRoles |= ChatRoles.SubscriberTier1;
      // TODO: Find out how to determine subscription tier

      if (message.SubscribedMonthCount >= 12)
      {
        userRoles |= ChatRoles.SubscriberOverTwelveMonths;
      }
      if (message.SubscribedMonthCount >= 6)
      {
        userRoles |= ChatRoles.SubscriberOverSixMonths;
      }
      if (message.SubscribedMonthCount >= 3)
      {
        userRoles |= ChatRoles.SubscriberOverThreeMonths;
      }
    }

    return userRoles;
  }

  public static bool IsUserPermitted(ChatRoles commandPermissions, ChatRoles userRoles)
  {
    if (commandPermissions == ChatRoles.None)
    {
      return true;
    }

    if (userRoles.HasOneOfFlags(ChatRoles.GlobalOperator, ChatRoles.ChannelOperator, ChatRoles.Broadcaster))
    {
      return true;
    }

    return ((int) commandPermissions & (int) userRoles) > 0;
  }
}
