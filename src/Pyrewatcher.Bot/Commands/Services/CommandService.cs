using Microsoft.Extensions.Configuration;
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

  private readonly Dictionary<string, ICommand> _stockCommands;

  private readonly IConfiguration _configuration;
  private readonly ILogger<CommandService> _logger;

  private readonly ICommandAliasesRepository _commandAliasesRepository;
  private readonly ICommandsRepository _commandsRepository;
  private readonly IOperatorsRepository _operatorsRepository;

  public CommandService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<CommandService> logger,
    ICommandAliasesRepository commandAliasesRepository, ICommandsRepository commandsRepository, IOperatorsRepository operatorsRepository)
  {
    _configuration = configuration;
    _logger = logger;
    _commandAliasesRepository = commandAliasesRepository;
    _commandsRepository = commandsRepository;
    _operatorsRepository = operatorsRepository;

    _stockCommands = Assembly.GetExecutingAssembly()
                             .GetTypes()
                             .Where(type => type.IsClass)
                             .Where(type => type.Name.EndsWith("Command"))
                             .Select(type => (ICommand) serviceProvider.GetService(type)!)
                             .ToDictionary(k => k.Keyword, v => v);
  }

  public void Initialize(BotInstance instance)
  {
    _instance = instance;
  }

  public async Task HandleCommand(ChatCommand chatCommand)
  {
    var chatMessage = chatCommand.ChatMessage;
    _logger.LogInformation("{User} issued command \"{Command}\" in channel {Channel}", chatMessage.DisplayName, chatMessage.Message, chatMessage.Channel);
    var timestampUtc = DateTime.UtcNow;
    var stopwatch = Stopwatch.StartNew();

    // 1. Check for an alias
    var alias = await _commandAliasesRepository.GetNewCommandByAliasAndChannelId($"{chatCommand.CommandIdentifier}{chatCommand.CommandText}",
      _instance.Channel.Id);
    // If command identifier is '!', then it's supported only if it's an alias
    if (alias is null && chatCommand.CommandIdentifier == '!')
    {
      _logger.LogInformation("Command starts with '!' and its alias was not found - returning");
      return;
    }

    var fullCommandAsList = (alias ?? chatCommand.CommandText).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                                              .Concat(chatCommand.ArgumentsAsList)
                                                              .ToList();

    _logger.LogDebug("Command being executed: \"\\{Command}\"", string.Join(' ', fullCommandAsList));

    // Check if command is available for the channel
    var command = await _commandsRepository.GetCommandForChannel(_instance.Channel.Id, fullCommandAsList.Take(2).ToArray());
    if (command is null)
    {
      _logger.LogInformation("Command was not found - returning");
      return;
    }

    _logger.LogDebug("Command found: \"\\{FullCommand}\", type: {Type}", command.FullCommand, command.Type.ToString());

    // Find command classes
    ICommand? commandClass = null;
    switch (command.Type)
    {
      case CommandType.Stock:
        _stockCommands.TryGetValue(command.Keyword, out commandClass);
        break;
      case CommandType.Custom:
        // TODO: Implement custom commands
        break;
    }

    if (commandClass is null)
    {
      _logger.LogWarning("Class instance for command \"\\{Command}\" does not exist - returning", command.Keyword);
      return;
    }

    _logger.LogDebug("Class instance for command \"\\{Command}\" found: {TypeName}", command.Keyword, commandClass.GetType().Name);

    ISubcommand? subcommandClass = null;
    if (command.Subkeyword is not null)
    {
      _logger.LogDebug("Looking for subcommand");
      var supercommand = (ICommandWithSubcommands) commandClass;
      switch (command.Type)
      {
        case CommandType.Stock:
          supercommand.Subcommands.TryGetValue(command.Keyword, out subcommandClass);
          break;
      }

      if (subcommandClass is null)
      {
        _logger.LogWarning("Class instance for subcommand \"\\{FullCommand}\" does not exist - returning", command.FullCommand);
        return;
      }

      _logger.LogDebug("Class instance for subcommand \"\\{FullCommand}\" found: {TypeName}", command.FullCommand, subcommandClass.GetType().Name);
    }

    // Prevent concurrent command executions
    // https://stackoverflow.com/questions/20084695/c-sharp-lock-and-async-method
    await commandClass.Semaphore.WaitAsync();
    _logger.LogDebug("Semaphore of command \"\\{Command}\" for channel {Channel} has been claimed", command.Keyword, chatMessage.Channel);
    try
    {
      // Attempt to execute command
      var executionResult = await ExecuteCommand(command, (subcommandClass is not null ? subcommandClass : commandClass)!, chatMessage, fullCommandAsList);

      stopwatch.Stop();
      if (executionResult.Result)
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
        CommandId = command.Id,
        InputCommand = $"{chatCommand.CommandIdentifier}{chatCommand.CommandText} {chatCommand.ArgumentsAsString}",
        ExecutedCommand = $"\\{string.Join(' ', fullCommandAsList)}",
        Result = executionResult.Result,
        TimestampUtc = timestampUtc,
        TimeInMilliseconds = stopwatch.ElapsedMilliseconds,
        Comment = executionResult.Comment
      };

      await _commandsRepository.LogCommandExecution(resultToLog);
    }
    finally
    {
      commandClass.Semaphore.Release();
      _logger.LogDebug("Semaphore of command \"\\{Command}\" for channel {Channel} has been released", command.Keyword, chatMessage.Channel);
    }
  }

  private async Task<CommandExecutionPartialResult> ExecuteCommand(Command commandInfo, IExecutableCommand commandClass, ChatMessage chatMessage,
    List<string> fullCommandAsList)
  {
    // Check if user is permitted to use the command
    var userRoles = await GetUserRoles(chatMessage);
    if (!IsUserPermitted(commandInfo.Permissions, userRoles))
    {
      _logger.LogInformation("User {User} has insufficient permissions to execute command \"\\{Command}\"", chatMessage.DisplayName,
        string.Join(' ', fullCommandAsList));
      _logger.LogDebug("User permissions: {UserPermissions}. Command requirement: {CommandRequirement}", userRoles, commandInfo.Permissions);
      return new CommandExecutionPartialResult
      {
        Result = false, Comment = $"Insufficient permissions (command required {commandInfo.Permissions}, user had {userRoles})"
      };
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
        _logger.LogInformation("Command \"\\{Command}\" is on cooldown - {SecondsLeft:F2}s left", commandInfo.Keyword, secondsLeft);
        return new CommandExecutionPartialResult
        {
          Result = false, Comment = $"Command on cooldown - {secondsLeft:F2}s left{(isUserOperator ? " (operator)" : string.Empty)}"
        };
      }
    }
    else
    {
      _logger.LogDebug("First execution of command \"\\{Command}\" on channel {Channel}", string.Join(' ', fullCommandAsList), chatMessage.Channel);
    }

    // Execute command
    return await commandClass.ExecuteAsync(fullCommandAsList.Skip(1).ToList(), chatMessage);
  }

  public async Task<ChatRoles> GetUserRoles(ChatMessage message)
  {
    // Ban/restriction check
    // This is something that might be implemented in the future

    var userRoles = ChatRoles.Viewer;

    // Database operator check
    userRoles |= await _operatorsRepository.GetUsersOperatorRoleByChannel(long.Parse(message.UserId), _instance.Channel.Id);

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

  public static bool IsUserPermitted(int commandPermissions, ChatRoles userRoles)
  {
    if (userRoles.HasOneOfFlags(ChatRoles.GlobalOperator, ChatRoles.ChannelOperator, ChatRoles.Broadcaster))
    {
      return true;
    }

    return (commandPermissions & (int) userRoles) > 0;
  }
}
