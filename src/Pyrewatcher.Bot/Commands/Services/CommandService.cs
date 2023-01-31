using Microsoft.Extensions.Configuration;
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

  private readonly ICommandAliasesRepository _commandAliasesRepository;
  private readonly ICommandsRepository _commandsRepository;
  private readonly IOperatorsRepository _operatorsRepository;

  public CommandService(IServiceProvider serviceProvider, IConfiguration configuration, ICommandAliasesRepository commandAliasesRepository,
    ICommandsRepository commandsRepository, IOperatorsRepository operatorsRepository)
  {
    _configuration = configuration;
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
    var timestampUtc = DateTime.UtcNow;
    var stopwatch = Stopwatch.StartNew();

    // 1. Check for an alias
    var alias = await _commandAliasesRepository.GetNewCommandByAliasAndChannelId($"{chatCommand.CommandIdentifier}{chatCommand.CommandText}",
                                                                                 _instance.Channel.Id);
    // If command identifier is '!', then it's supported only if it's an alias
    if (alias is null && chatCommand.CommandIdentifier == '!')
    {
      // TODO: Log
      return;
    }

    var fullCommandAsList = (alias ?? chatCommand.CommandText).Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                                              .Concat(chatCommand.ArgumentsAsList)
                                                              .ToList();

    // Check if command is available for the channel
    var command = await _commandsRepository.GetCommandForChannel(_instance.Channel.Id, fullCommandAsList.Take(2).ToArray());
    if (command is null)
    {
      // TODO: Log
      return;
    }

    // Find command classes
    var commandClass = command.Type switch
    {
      CommandType.Stock => _stockCommands[command.Keyword],
      // TODO: Implement custom commands
      CommandType.Custom => null,
      _ => null // this will never happen anyway
    };

    if (commandClass is null)
    {
      return;
    }

    var subcommandClass = commandClass is ICommandWithSubcommands supercommand
      ? command.Type switch
      {
        CommandType.Stock => command.Subkeyword is not null ? supercommand.Subcommands[command.Subkeyword] : null,
        CommandType.Custom => null,
        _ => null // this will never happen anyway
      }
      : null;

    // Prevent concurrent command executions
    // https://stackoverflow.com/questions/20084695/c-sharp-lock-and-async-method
    await commandClass.Semaphore.WaitAsync();
    try
    {
      // Attempt to execute command
      var executionResult = await ExecuteCommand(command, commandClass, chatCommand.ChatMessage, fullCommandAsList);

      stopwatch.Stop();

      var resultToLog = new CommandExecution
      {
        ChannelId = _instance.Channel.Id,
        UserId = long.Parse(chatCommand.ChatMessage.UserId),
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
    }
  }

  private async Task<CommandExecutionPartialResult> ExecuteCommand(Command commandInfo, IExecutableCommand commandClass, ChatMessage chatMessage,
    List<string> fullCommandAsList)
  {
    // Check if user is permitted to use the command
    var userRoles = await GetUserRoles(chatMessage);
    if (!IsUserPermitted(commandInfo.Permissions, userRoles))
    {
      // TODO: Log
      return new CommandExecutionPartialResult
      {
        Result = false, Comment = $"Insufficient permissions (command required {commandInfo.Permissions}, user had {userRoles})"
      };
    }

    // Check if command is on cooldown
    var isUserOperator = userRoles.HasOneOfFlags(ChatRoles.GlobalOperator, ChatRoles.ChannelOperator);
    // Cooldown for operators is no longer than 2 seconds
    var userCooldown = TimeSpan.FromSeconds(isUserOperator ? Math.Min(commandInfo.CooldownInSeconds, 2) : commandInfo.CooldownInSeconds);

    if (commandInfo.LatestExecutionUtc is not null)
    {
      var lastUsage = DateTime.UtcNow - commandInfo.LatestExecutionUtc.Value;

      if (lastUsage < userCooldown)
      {
        // TODO: Log
        var secondsLeft = (userCooldown - lastUsage).TotalMilliseconds / 1000.0;
        return new CommandExecutionPartialResult
        {
          Result = false, Comment = $"Command on cooldown - {secondsLeft:F2}s left{(isUserOperator ? " (operator)" : string.Empty)}"
        };
      }
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
    var operatorRole = await _operatorsRepository.GetUsersOperatorRoleByChannel(long.Parse(message.UserId), _instance.Channel.Id);
    if (operatorRole is not null)
    {
      userRoles |= operatorRole.Value;
    }

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
