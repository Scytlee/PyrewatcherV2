using Serilog.Events;

namespace Pyrewatcher.Bot.Serilog;

public static class MyFunctions
{
  public static LogEventPropertyValue? ShortenTwitchLib(LogEventPropertyValue? message)
  {
    return message is ScalarValue { Value: string s } 
      ? new ScalarValue(s.StartsWith("[TwitchLib") 
                          ? $"[IRC] {s[(s.IndexOf(']') + 2)..]}" 
                          : s)
      : null;
  }
  
  public static LogEventPropertyValue? ShortenTypeName(LogEventPropertyValue? typeName)
  {
    if (typeName is not ScalarValue { Value: string s } )
    {
      return null;
    }
    
    var lastIndexOfDot = s.LastIndexOf('.');
    return new ScalarValue(lastIndexOfDot == -1 ? s : s[(lastIndexOfDot + 1)..]);
  }
}
