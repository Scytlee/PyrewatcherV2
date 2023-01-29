namespace Pyrewatcher.Library.Utilities;

public static class StringExtensions
{
  public static string Normalize(this string str) => str.Replace(" ", "").ToLower();
}
