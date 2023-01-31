using Pyrewatcher.Library.Enums;

namespace Pyrewatcher.Library.Utilities;

public static class ChatRolesExtensions
{
  public static bool HasOneOfFlags(this ChatRoles userRoles, params ChatRoles[] roles) => roles.Any(role => (userRoles & role) > 0);
}
