using Pyrewatcher.Library.Models;

namespace Pyrewatcher.Library.DataAccess.Interfaces;

public interface ILanguageRepository
{
  Task<Result<Dictionary<string, string>>> GetLanguage(string languageCode);
}
