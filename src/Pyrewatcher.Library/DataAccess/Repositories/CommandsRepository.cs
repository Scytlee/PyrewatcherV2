using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.DataAccess.InternalModels;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class CommandsRepository : ICommandsRepository
{
  private readonly IDapperService _dapperService;
  private readonly ILogger<CommandsRepository> _logger;

  public CommandsRepository(IDapperService dapperService, ILogger<CommandsRepository> logger)
  {
    _dapperService = dapperService;
    _logger = logger;
  }

  public async Task<Result<Command?>> GetCommandForChannel(long channelId, IEnumerable<string> commandAsList)
  {
    // This query retrieves a tree for given command keywords
    // Example: For command 'account list', this query will return commands 'account list' and 'account'
    // The assumption is that the query will always return a full tree, but I didn't test it enough to confirm it
    const string query = """
WITH [CommandKeywords] AS (
  SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS [Level], [value] AS [Keyword]
  FROM STRING_SPLIT(@command, ' ')
), [CommandTree] AS (
  SELECT *, 1 AS [Level]
  FROM [Core].[Commands]
  WHERE [Keyword] = (SELECT [Keyword] FROM [CommandKeywords] WHERE [Level] = 1)
    AND ([ChannelId] IS NULL OR [ChannelId] = @channelId)
  UNION ALL
  SELECT [c].*, [ct].[Level] + 1
  FROM [Core].[Commands] [c]
  INNER JOIN [CommandTree] [ct] ON [ct].[Id] = [c].[ParentCommandId]
  WHERE [c].[Keyword] = (SELECT [Keyword] FROM [CommandKeywords] WHERE [Level] = [ct].[Level] + 1)
    AND [ct].[Keyword] = (SELECT [Keyword] FROM [CommandKeywords] WHERE [Level] = [ct].[Level])
    AND ([c].[ChannelId] IS NULL OR [c].[ChannelId] = @channelId)
), [LatestExecution] AS (
  SELECT MAX([ce].[TimestampUtc]) AS [LatestExecutionUtc]
  FROM [CommandTree] [c]
  LEFT JOIN [Log].[CommandExecutions] [ce] ON [ce].[CommandId] = [c].[Id] AND [ce].[ChannelId] = @channelId AND [ce].[Result] = 1
)
SELECT [c].[Id], [c].[Keyword], [c].[Type], [c].[Level], [ce].[LatestExecutionUtc], [ct].[Text] AS [CustomText],
       CASE WHEN [cd].[Enabled] IS NULL THEN NULL ELSE COALESCE([co].[Enabled], [cd].[Enabled]) END AS [Enabled],
       CASE WHEN [cd].[CooldownInSeconds] IS NULL THEN NULL ELSE COALESCE([co].[CooldownInSeconds], [cd].[CooldownInSeconds]) END AS [CooldownInSeconds],
       CASE WHEN [cd].[Permissions] IS NULL THEN NULL ELSE COALESCE([co].[Permissions], [cd].[Permissions]) END AS [Permissions]
FROM [CommandTree] [c]
LEFT JOIN [Command].[Overrides] [co] ON [co].[CommandId] = [c].[Id] AND [co].[ChannelId] = @channelId
LEFT JOIN [Command].[Defaults] [cd] ON [cd].[CommandId] = [c].[Id]
LEFT JOIN [LatestExecution] [ce] ON [c].[Level] = 1
LEFT JOIN [Command].[CustomText] [ct] ON [ct].[CommandId] = [c].[Id]
ORDER BY [Level] DESC;
""";
    
    var dbResult = await _dapperService.QueryAsync<CommandInternal>(query, new { channelId, command = string.Join(' ', commandAsList) });
    if (!dbResult.IsSuccess)
    {
      _logger.LogError(dbResult.Exception, "An error occurred during execution of {MethodName} query", nameof(GetCommandForChannel));
      return Result<Command?>.Failure();
    }
    _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(GetCommandForChannel), dbResult.ExecutionTime);
    
    // Commands are ordered from most significant (account list) to least significant (account)
    var commands = dbResult.Content!.ToList(); 

    // If nothing has been found, return null
    if (!commands.Any())
    {
      return Result<Command?>.Success(null);
    }

    // If any command's Enabled or Permissions field is null, return null
    // Reason: If above statement holds, then there is at least one command without specified
    // default values, therefore it's considered unfinished
    if (commands.Any(command => command.Enabled is null || command.Permissions is null))
    {
      return Result<Command?>.Success(null);
    }

    // If the most significant command is disabled, return null
    if (!commands.First().Enabled!.Value)
    {
      return Result<Command?>.Success(null);
    }

    // If the least significant command's cooldown is null, return null
    // Reason: All subcommands of a specific command share cooldown with the main command
    if (commands.Last().CooldownInSeconds is null)
    {
      return Result<Command?>.Success(null);
    }

    // Assemble the command
    var command = new Command
    {
      Id = commands.First().Id,
      ParentId = commands.Count > 1 ? commands.Last().Id : null,
      PriorKeywords = commands.Skip(1).Select(command => command.Keyword).Reverse().ToArray(),
      Keyword = commands.First().Keyword,
      Type = commands.First().Type,
      CooldownInSeconds = commands.Last().CooldownInSeconds!.Value,
      Permissions = commands.First().Permissions!.Value,
      LatestExecutionUtc = commands.Last().LatestExecutionUtc,
      CustomText = commands.First().CustomText
    };

    return Result<Command?>.Success(command);
  }

  public async Task<Result<bool>> LogCommandExecution(CommandExecution execution)
  {
    const string query = """
INSERT INTO [Log].[CommandExecutions] ([ChannelId], [UserId], [CommandId], [InputCommand],
                                       [ExecutedCommand], [Result], [TimestampUtc],
                                       [TimeInMilliseconds], [Comment])
VALUES (@ChannelId, @UserId, @CommandId, @InputCommand, @ExecutedCommand, @Result, @TimestampUtc, @TimeInMilliseconds, @Comment);
""";

    var dbResult = await _dapperService.ExecuteAsync(query, execution);
    if (!dbResult.IsSuccess)
    {
      _logger.LogError(dbResult.Exception, "An error occurred during execution of {MethodName} query", nameof(LogCommandExecution));
      return Result<bool>.Failure();
    }
    
    _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(LogCommandExecution), dbResult.ExecutionTime);
    return Result<bool>.Success(dbResult.Content == 1);
  }
}
