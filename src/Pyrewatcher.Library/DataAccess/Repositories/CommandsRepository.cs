using Dapper;
using Microsoft.Extensions.Configuration;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.DataAccess.InternalModels;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class CommandsRepository : RepositoryBase, ICommandsRepository
{
  public CommandsRepository(IConfiguration configuration) : base(configuration)
  {
  }

  public async Task<Command?> GetCommandForChannel(long channelId, params string[] keywords)
  {
    // This query retrieves a tree for given command keywords
    // Example: For command 'account list', this query will return commands 'account list' and 'account'
    // The assumption is that the query will always return a full tree, but I didn't test it enough to confirm it
    const string query = """
WITH [CommandTree] AS (
  SELECT *, 1 AS [Degree]
  FROM [Core].[Commands]
  WHERE [Keyword] = @firstKeyword AND ([ChannelId] IS NULL OR [ChannelId] = @channelId)
  UNION ALL
  SELECT [c].*, [ct].[Degree] + 1
  FROM [Core].[Commands] [c]
  INNER JOIN [CommandTree] [ct] ON [ct].[Id] = [c].[ParentCommandId]
  WHERE [c].[Keyword] = @secondKeyword AND [ct].[Keyword] = @firstKeyword AND ([c].[ChannelId] IS NULL OR [c].[ChannelId] = @channelId)
), [LatestExecutions] AS (
  SELECT [c].[Id], MAX([ce].[TimestampUtc]) AS [LatestExecutionUtc]
  FROM [CommandTree] [c]
  LEFT JOIN [Log].[CommandExecutions] [ce] ON [ce].[CommandId] = [c].[Id] AND [ce].[ChannelId] = @channelId AND [ce].[Result] = 1
  GROUP BY [c].[Id]
)
SELECT [c].[Id], [c].[Keyword], [c].[Type], [c].[Degree], [ce].[LatestExecutionUtc], [ct].[Text] AS [CustomText],
       CASE WHEN [cd].[Enabled] IS NULL THEN NULL ELSE COALESCE([co].[Enabled], [cd].[Enabled]) END AS [Enabled],
       CASE WHEN [cd].[CooldownInSeconds] IS NULL THEN NULL ELSE COALESCE([co].[CooldownInSeconds], [cd].[CooldownInSeconds]) END AS [CooldownInSeconds],
       CASE WHEN [cd].[Permissions] IS NULL THEN NULL ELSE COALESCE([co].[Permissions], [cd].[Permissions]) END AS [Permissions]
FROM [CommandTree] [c]
LEFT JOIN [Command].[Overrides] [co] ON [co].[CommandId] = [c].[Id] AND [co].[ChannelId] = @channelId
LEFT JOIN [Command].[Defaults] [cd] ON [cd].[CommandId] = [c].[Id]
LEFT JOIN [LatestExecutions] [ce] ON [ce].[Id] = [c].[Id]
LEFT JOIN [Command].[CustomText] [ct] ON [ct].[CommandId] = [c].[Id]
ORDER BY [Degree] DESC;
""";

    using var connection = await CreateConnection();

    // Commands are ordered from most significant (account list) to least significant (account)
    var commands = (await connection.QueryAsync<CommandInternal>(
      query, new { channelId, firstKeyword = keywords[0], secondKeyword = keywords.Length > 1 ? keywords[1] : null })).ToList();

    // If nothing has been found, return null
    if (!commands.Any())
    {
      return null;
    }
    
    // If any command's Enabled or Permissions field is null, return null
    // Reason: If above statement holds, then there is at least one command without specified
    // default values, therefore it's considered unfinished
    if (commands.Any(command => command.Enabled is null || command.Permissions is null))
    {
      return null;
    }
    
    // If the most significant command is disabled, return null
    if (!commands.First().Enabled!.Value)
    {
      return null;
    }
    
    // If the least significant command's cooldown is null, return null
    // Reason: All subcommands of a specific command share cooldown with the main command
    if (commands.Last().CooldownInSeconds is null)
    {
      return null;
    }

    // Assemble the command
    var command = new Command
    {
      Id = commands.First().Id,
      ParentId = commands.Count > 1 ? commands.Last().Id : null,
      Keyword = commands.First().Keyword,
      Subkeyword = commands.Count > 1 ? commands.Last().Keyword : null,
      Type = commands.First().Type,
      CooldownInSeconds = commands.Last().CooldownInSeconds!.Value,
      Permissions = commands.First().Permissions!.Value,
      LatestExecutionUtc = commands.Last().LatestExecutionUtc,
      CustomText = commands.First().CustomText
    };
    
    return command;
  }

  public async Task<bool> LogCommandExecution(CommandExecution execution)
  {
    const string query = """
INSERT INTO [Log].[CommandExecutions] ([ChannelId], [UserId], [CommandId], [InputCommand],
                                       [ExecutedCommand], [Result], [TimestampUtc],
                                       [TimeInMilliseconds], [Comment])
VALUES (@ChannelId, @UserId, @CommandId, @InputCommand, @ExecutedCommand, @Result, @TimestampUtc, @TimeInMilliseconds, @Comment);
""";
    
    using var connection = await CreateConnection();

    var result = await connection.ExecuteAsync(query, execution);

    return result == 1;
  }
}
