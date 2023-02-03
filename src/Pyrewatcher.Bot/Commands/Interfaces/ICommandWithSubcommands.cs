namespace Pyrewatcher.Bot.Commands.Interfaces;

public interface ICommandWithSubcommands : ICommand
{
  Dictionary<string, ICommand> Subcommands { get; }

  void InitializeSubcommands(IEnumerable<ICommand> subcommands);
}
