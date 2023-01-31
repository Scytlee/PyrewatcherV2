namespace Pyrewatcher.Bot.Commands.Interfaces;

public interface ICommandWithSubcommands : ICommand
{
  Dictionary<string, ISubcommand> Subcommands { get; }

  void InitializeSubcommands(IEnumerable<ISubcommand> subcommands);
}
