namespace Pyrewatcher.Bot.Commands.Interfaces;

public interface ILockable
{
  SemaphoreSlim Semaphore { get; }
}
