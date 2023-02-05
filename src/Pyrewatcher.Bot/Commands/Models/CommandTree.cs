using Pyrewatcher.Bot.Commands.Interfaces;
using System.Collections.ObjectModel;

namespace Pyrewatcher.Bot.Commands.Models;

public class CommandTree
{
  private readonly List<CommandTree> _children = new();
  
  public CommandTree Root { get; }
  public CommandTree? Parent { get; }
  public ICommand Value { get; }
  public ReadOnlyCollection<CommandTree> Children => _children.AsReadOnly();

  public CommandTree(ICommand value)
  {
    Root = this;
    Parent = null;
    Value = value;
  }
  
  public CommandTree(CommandTree parent, ICommand value)
  {
    Root = parent.Root;
    Parent = parent;
    Value = value;
  }

  public CommandTree AddChild(ICommand value)
  {
    var node = new CommandTree(this, value);
    _children.Add(node);
    return node;
  }

  public CommandTree? FindChild(Func<CommandTree, bool> predicate)
  {
    if (predicate(this))
    {
      return this;
    }

    foreach (var child in _children)
    {
      var result = child.FindChild(predicate);
      if (result is not null)
      {
        return result;
      }
    }

    return null;
  }
}
