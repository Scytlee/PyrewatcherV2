namespace Pyrewatcher.Bot.Commands.Interfaces;

public interface ISubcommand : IExecutableCommand
{
  string Subkeyword { get; }
}
