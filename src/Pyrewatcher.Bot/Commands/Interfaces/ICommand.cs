namespace Pyrewatcher.Bot.Commands.Interfaces;

public interface ICommand : IExecutableCommand
{
  string Keyword { get; }
  SemaphoreSlim Semaphore { get; }
}
