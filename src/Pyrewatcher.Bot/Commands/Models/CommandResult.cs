namespace Pyrewatcher.Bot.Commands.Models;

public record CommandResult(bool IsSuccess, string? Comment)
{
  public static readonly CommandResult Success = new(true, null);
  public static CommandResult Failure(string comment) => new(false, comment);
}