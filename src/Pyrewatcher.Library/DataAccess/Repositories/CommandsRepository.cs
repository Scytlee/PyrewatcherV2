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
    // TODO: This query (and method probably) could be improved a lot, but it works for now
    // This query retrieves a tree for given command keywords (up to 2)
    // Example: For command 'account list', this query will return commands 'account list' and 'account'
    // The assumption is that the query will always return a full tree, but I didn't test it enough to confirm it
    const string query = """
WITH [CommandTree] AS (
  SELECT [c].*, [co].[Enabled], [co].[CooldownInSeconds], [co].[Permissions]
  FROM (
    SELECT [c1].[Id], [c1].[Keyword], [c2].[Type], 2 AS [Degree]
    FROM [Core].[Commands] [c1]
    LEFT JOIN [Core].[Commands] [c2] ON [c2].[Id] = [c1].[ParentCommandId]
    WHERE [c1].[Keyword] = @secondKeyword AND [c2].[Keyword] = @firstKeyword AND ([c1].[ChannelId] IS NULL OR [c1].[ChannelId] = @channelId)
    UNION
    SELECT [c1].[Id], [c1].[Keyword], [c1].[Type], 1 AS [Degree]
    FROM [Core].[Commands] [c1]
    WHERE [c1].[Keyword] = @firstKeyword AND ([c1].[ChannelId] IS NULL OR [c1].[ChannelId] = @channelId)
  ) [c]
  LEFT JOIN [Command].[Overrides] [co] ON [co].[CommandId] = [c].[Id] AND [co].[ChannelId] = @channelId
), [LatestExecutions] AS (
  SELECT [c].[Id], MAX([ce].[TimestampUtc]) AS [LatestExecutionUtc]
  FROM [CommandTree] [c]
  LEFT JOIN [Log].[CommandExecutions] [ce] ON [ce].[CommandId] = [c].[Id] AND [ce].[ChannelId] = @channelId AND [ce].[Result] = 1
  GROUP BY [c].[Id]
)
SELECT [c].[Id], [c].[Keyword], [c].[Type], [c].[Degree], [ce].[LatestExecutionUtc], [ct].[Text] AS [CustomText],
       CASE WHEN [cd].[Enabled] IS NULL THEN NULL ELSE COALESCE([c].[Enabled], [cd].[Enabled]) END AS [Enabled],
       CASE WHEN [cd].[CooldownInSeconds] IS NULL THEN NULL ELSE COALESCE([c].[CooldownInSeconds], [cd].[CooldownInSeconds]) END AS [CooldownInSeconds],
       CASE WHEN [cd].[Permissions] IS NULL THEN NULL ELSE COALESCE([c].[Permissions], [cd].[Permissions]) END AS [Permissions]
FROM [CommandTree] [c]
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
