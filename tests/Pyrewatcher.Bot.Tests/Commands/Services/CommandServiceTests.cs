using FluentAssertions;
using NUnit.Framework;
using Pyrewatcher.Bot.Commands.Services;
using Pyrewatcher.Library.Enums;

namespace Pyrewatcher.Bot.Tests.Commands.Services;

[TestFixture]
public class CommandServiceTests
{
  [TestCase(ChatRoles.None, ChatRoles.Viewer, true)]
  [TestCase(ChatRoles.Viewer, ChatRoles.Viewer, true)]
  [TestCase(ChatRoles.SubscriberTier1, ChatRoles.Viewer, false)]
  [TestCase(ChatRoles.Vip, ChatRoles.Viewer, false)]
  [TestCase(ChatRoles.Moderator, ChatRoles.Viewer, false)]
  [TestCase(ChatRoles.Broadcaster, ChatRoles.Viewer, false)]
  [TestCase(ChatRoles.ChannelTrusted, ChatRoles.Viewer, false)]
  [TestCase(ChatRoles.ChannelOperator, ChatRoles.Viewer, false)]
  [TestCase(ChatRoles.GlobalOperator, ChatRoles.Viewer, false)]
  public void IsUserPermitted_Viewer(ChatRoles commandPermissions, ChatRoles userRoles, bool expected)
  {
    // arrange
    // nothing to arrange here

    // act
    var actual = CommandService.IsUserPermitted(commandPermissions, userRoles);

    // assert
    actual.Should().Be(expected);
  }
}
