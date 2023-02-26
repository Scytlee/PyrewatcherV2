namespace Pyrewatcher.Bot.Interfaces;

public interface IMessageGenerator
{
  Task<string?> Generate(string templateName, string languageCode, object? parameters = null, string? mention = null);
}
