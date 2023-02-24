namespace Pyrewatcher.Library.Models;

public record DbResult<T>(bool IsSuccess, T? Content, long? ExecutionTime, Exception? Exception)
{
  public static DbResult<T> Success(T content, long executionTime) => new(true, content, executionTime, null);

  public static DbResult<T> Failure(Exception? exception = null) => new(false, default(T), null, exception);
}
