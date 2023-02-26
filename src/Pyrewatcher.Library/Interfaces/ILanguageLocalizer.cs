namespace Pyrewatcher.Library.Interfaces;

public interface ILanguageLocalizer
{
  Task<string?> Get(string templateName, string languageCode);
}