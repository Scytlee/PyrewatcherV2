using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Interfaces;

namespace Pyrewatcher.Library.Services;

public class LanguageLocalizer : ILanguageLocalizer
{
  private readonly Dictionary<string, Dictionary<string, string>> _languages;
  private readonly ILanguageRepository _languageRepository;

  public LanguageLocalizer(ILanguageRepository languageRepository)
  {
    _languageRepository = languageRepository;
    _languages = new Dictionary<string, Dictionary<string, string>>();
  }

  public async Task<string?> Get(string templateName, string languageCode)
  {
    if (!_languages.ContainsKey(languageCode))
    {
      var languageResult = await _languageRepository.GetLanguage(languageCode);
      if (!languageResult.IsSuccess)
      {
        return null;
      }
      _languages.Add(languageCode.ToLower(), languageResult.Content!);
    }

    return _languages[languageCode].TryGetValue(templateName, out var template) ? template : null;
  }
}
