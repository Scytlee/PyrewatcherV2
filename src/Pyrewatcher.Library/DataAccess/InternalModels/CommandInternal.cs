using Pyrewatcher.Library.Enums;

namespace Pyrewatcher.Library.DataAccess.InternalModels;

internal class CommandInternal 
{
  public long Id { get; set; }
  public string Keyword { get; set; }
  public CommandType Type { get; set; }
  public bool? Enabled { get; set; }
  public int? CooldownInSeconds { get; set; }
  public ChatRoles? Permissions { get; set; }
  public DateTime? LatestExecutionUtc { get; set; }
  public string? CustomText { get; set; }
}
