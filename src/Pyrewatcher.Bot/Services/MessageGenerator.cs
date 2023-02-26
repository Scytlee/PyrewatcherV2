using Microsoft.Extensions.Logging;
using Pyrewatcher.Bot.Interfaces;
using Pyrewatcher.Library.Interfaces;
using SmartFormat;

namespace Pyrewatcher.Bot.Services;

public class MessageGenerator : IMessageGenerator
{
  private readonly ILanguageLocalizer _localizer;
  private readonly ILogger<MessageGenerator> _logger;

  public MessageGenerator(ILanguageLocalizer localizer, ILogger<MessageGenerator> logger)
  {
    _localizer = localizer;
    _logger = logger;
  }

  public async Task<string?> Generate(string templateName, string languageCode, object? parameters = null, string? mention = null)
  {
    // Retrieve message template
    var template = await _localizer.Get(templateName, languageCode);
    if (template is null)
    {
      _logger.LogWarning("Template \"{Template}\" for language \"{Language}\" could not be found", templateName, languageCode);
      return null;
    }
    
    try
    {
      // Format message
      var message = Smart.Format(template, parameters);
      
      // Optional modifications
      if (mention is not null)
      {
        message = $"@{mention} " + message;
      }

      // Return
      return message;
    }
    catch (Exception exception)
    {
      _logger.LogWarning(exception, "Message could not be generated");
      return null;
    }
  }
}
