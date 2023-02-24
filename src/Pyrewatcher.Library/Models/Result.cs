namespace Pyrewatcher.Library.Models;

/// <summary>
/// A record representing result of a request, e.g. repository query.
/// </summary>
/// <param name="IsSuccess">True if request was handled successfully, false if an error outside of the application occurred.</param>
/// <param name="Content">Content of the request, or fallback value in case of a failure.</param>
/// <param name="ErrorMessage">Optional error message in case of a failure.</param>
/// <typeparam name="T">Type of request's content.</typeparam>
public record Result<T>(bool IsSuccess, T? Content, string? ErrorMessage)
{
  /// <summary>
  /// Creates a new instance of the Result class indicating that the request was successful.
  /// </summary>
  /// <param name="content">Content of the request.</param>
  /// <returns>A new instance of the Result class.</returns>
  public static Result<T> Success(T? content) => new(true, content, null);

  /// <summary>
  /// Creates a new instance of the Result class indicating that the request was unsuccessful.<br/>
  /// Note: null or empty content does not mean that the result should be a failure!
  /// </summary>
  /// <param name="content">Optional content of the request.</param>
  /// <param name="errorMessage">Optional error message.</param>
  /// <returns>A new instance of the Result class.</returns>
  public static Result<T> Failure(T? content = default, string? errorMessage = null) => new(false, content, errorMessage);
}
