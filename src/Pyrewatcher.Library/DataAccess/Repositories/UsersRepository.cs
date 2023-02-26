using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class UsersRepository : IUsersRepository
{
  private readonly IDapperService _dapperService;
  private readonly ILogger<UsersRepository> _logger;

  public UsersRepository(IDapperService dapperService, ILogger<UsersRepository> logger)
  {
    _dapperService = dapperService;
    _logger = logger;
  }
  
  public async Task<Result<None>> UpsertUser(long id, string displayName)
  {
    const string query = """
DECLARE @currentDisplayName NVARCHAR(25);
DECLARE @currentNormalizedName NVARCHAR(25);
DECLARE @normalizedName NVARCHAR(25) = LOWER(@displayName);

SELECT @currentDisplayName = [DisplayName], @currentNormalizedName = [NormalizedName]
FROM [Core].[Users]
WHERE [Id] = @id;

IF @currentDisplayName IS NULL
BEGIN
  INSERT INTO [Core].[Users] ([Id], [DisplayName], [NormalizedName])
  VALUES (@id, @displayName, @normalizedName);

  RETURN;
END

IF @currentNormalizedName != @normalizedName
BEGIN
  INSERT INTO [Log].[UsernameChanges] ([UserId], [ObservedChangeTimestampUtc], [OldDisplayName], [OldNormalizedName])
  VALUES (@id, GETUTCDATE(), @currentDisplayName, @currentNormalizedName);
  
  UPDATE [Core].[Users]
  SET [DisplayName] = @displayName, [NormalizedName] = @normalizedName
  WHERE [Id] = @id;

  RETURN;
END

IF @currentDisplayName COLLATE Latin1_General_CS_AS != @displayName COLLATE Latin1_General_CS_AS
BEGIN
  UPDATE [Core].[Users]
  SET [DisplayName] = @displayName
  WHERE [Id] = @id;
END
""";

    var dbResult = await _dapperService.ExecuteAsync(query, new { id, displayName });
    if (!dbResult.IsSuccess)
    {
      _logger.LogError(dbResult.Exception, "An error occurred during execution of {MethodName} query", nameof(UpsertUser));
      return Result<None>.Failure();
    }
    
    _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(UpsertUser), dbResult.ExecutionTime);
    return Result<None>.Success(None.Value);
  }
}
