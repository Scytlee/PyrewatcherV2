namespace Pyrewatcher.Library.Enums;

[Flags]
public enum ChatRoles
{
  None = 0,
  
  // Twitch roles
  Viewer = 1 << 0,
  SubscriberTier1 = 1 << 1,
  SubscriberTier2 = 1 << 2,
  SubscriberTier3 = 1 << 3,
  SubscriberOverThreeMonths = 1 << 4,
  SubscriberOverSixMonths = 1 << 5,
  SubscriberOverTwelveMonths = 1 << 6,
  Vip = 1 << 7,
  Moderator = 1 << 8,
  Broadcaster = 1 << 9,
  
  // Bot custom roles
  ChannelTrusted = 1 << 10,
  ChannelOperator = 1 << 11,
  GlobalOperator = 1 << 12
}
