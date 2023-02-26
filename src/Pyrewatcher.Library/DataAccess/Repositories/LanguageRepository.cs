using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Repositories;

public class LanguageRepository : ILanguageRepository
{
  private readonly IDapperService _dapperService;
  private readonly ILogger<LanguageRepository> _logger;

  public LanguageRepository(IDapperService dapperService, ILogger<LanguageRepository> logger)
  {
    _dapperService = dapperService;
    _logger = logger;
  }

  public async Task<Result<Dictionary<string, string>>> GetLanguage(string languageCode)
  {
    const string query = """
SELECT [Name], [Value]
FROM [Language].[Resources]
WHERE [IsoCode] = @languageCode;
""";

    var dbResult = await _dapperService.QueryAsync<(string Name, string Value)>(query, new { languageCode });
    if (!dbResult.IsSuccess)
    {
      _logger.LogError(dbResult.Exception, "An error occurred during execution of {MethodName} query", nameof(GetLanguage));
      return Result<Dictionary<string, string>>.Failure();
    }
    
    _logger.LogTrace("{MethodName} query execution time: {Time} ms", nameof(GetLanguage), dbResult.ExecutionTime);
    return Result<Dictionary<string, string>>.Success(dbResult.Content!.ToDictionary(k => k.Name, v => v.Value));
  }
}
