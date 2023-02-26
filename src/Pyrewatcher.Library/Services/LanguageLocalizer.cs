using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Interfaces;

namespace Pyrewatcher.Library.Services;

public class LanguageLocalizer : ILanguageLocalizer
{
  private readonly Dictionary<string, Dictionary<string, string>> _languages;
  private readonly ILanguageRepository _languageRepository;
  private readonly ILogger<LanguageLocalizer> _logger;

  public LanguageLocalizer(ILanguageRepository languageRepository, ILogger<LanguageLocalizer> logger)
  {
    _languageRepository = languageRepository;
    _logger = logger;

    _languages = new Dictionary<string, Dictionary<string, string>>();
  }

  public async Task<string?> Get(string templateName, string languageCode)
  {
    // Check if language resources have been loaded
    if (!_languages.ContainsKey(languageCode)) // Not loaded
    {
      // Load resources
      var languageResult = await _languageRepository.LoadLanguageResources(languageCode);
      if (!languageResult.IsSuccess)
      {
        return null;
      }
      if (!languageResult.Content!.Any())
      {
        _logger.LogWarning("No resources for language \"{Language}\" have been found", languageCode);
      }
      _languages.Add(languageCode.ToLower(), languageResult.Content!);
    }

    return _languages[languageCode]!.TryGetValue(templateName, out var template) ? template : null;
  }
}
