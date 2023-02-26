namespace Pyrewatcher.Library.Models;

public readonly struct None : IEquatable<None>
{
  private static readonly None _value = new();

  public static ref readonly None Value => ref _value;

  public bool Equals(None other) => true;

  public override bool Equals(object? obj) => obj is None;

  public override int GetHashCode() => 0;

  public static bool operator ==(None first, None second) => true;

  public static bool operator !=(None first, None second) => false;

  public override string ToString() => "None";
}
