namespace Pyrewatcher.Library.Riot.Utilities;

public static class RiotUtilities
{
  private static DateTimeOffset GetStartTimeOffset()
  {
    if (DateTime.UtcNow - DateTime.Today < TimeSpan.FromHours(4))
    {
      var yesterday = DateTime.Today.Subtract(TimeSpan.FromDays(1));

      return new DateTimeOffset(yesterday.Year, yesterday.Month, yesterday.Day, 4, 00, 00, TimeSpan.Zero);
    }
    else
    {
      var today = DateTime.Today;

      return new DateTimeOffset(today.Year, today.Month, today.Day, 4, 00, 00, TimeSpan.Zero);
    }
  }

  public static long GetStartTimeInSeconds() => GetStartTimeOffset().ToUnixTimeSeconds();
}
