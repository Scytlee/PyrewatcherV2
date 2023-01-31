namespace Pyrewatcher.Library.Models;

public class CommandExecution
{
  public long ChannelId { get; set; }
  public long UserId { get; set; }
  public long CommandId { get; set; }
  public string InputCommand { get; set; } = string.Empty;
  public string ExecutedCommand { get; set; } = string.Empty;
  public bool Result { get; set; }
  public DateTime TimestampUtc { get; set; }
  public long TimeInMilliseconds { get; set; }
  public string? Comment { get; set; }
}
