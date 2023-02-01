using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pyrewatcher.Library.DataAccess.Interfaces;
using Pyrewatcher.Library.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace Pyrewatcher.Bot;

/// <summary>
/// Class responsible for all instances of the bot for all channels.
/// This class should be a singleton.
/// </summary>
public class BotInstanceManager
{
  private readonly Dictionary<string, BotInstance> _instances;

  private readonly IServiceProvider _serviceProvider;
  private readonly TwitchClient _client;
  private readonly IConfiguration _configuration;
  private readonly ILogger<BotInstanceManager> _logger;

  private readonly IChannelsRepository _channelsRepository;

  // private readonly SubscriptionService _subscriptionService;

  public BotInstanceManager(IServiceProvider serviceProvider, TwitchClient client, IConfiguration configuration, ILogger<BotInstanceManager> logger,
    IChannelsRepository channelsRepository)
  {
    _instances = new();

    _serviceProvider = serviceProvider;
    _client = client;
    _configuration = configuration;
    _logger = logger;
    _channelsRepository = channelsRepository;
  }

  public async Task Initialize()
  {
    _client.OnMessageReceived += OnMessageReceived;
    _client.OnJoinedChannel += OnJoinedChannel;
    _client.OnConnected += OnConnected;
    _client.OnFailureToReceiveJoinConfirmation += OnFailureToReceiveJoinConfirmation;
    _client.OnChatCommandReceived += OnChatCommandReceived;
    _client.OnCommunitySubscription += OnCommunitySubscription;
    _client.OnConnectionError += OnConnectionError;
    _client.OnDisconnected += OnDisconnected;
    _client.OnError += OnError;
    _client.OnGiftedSubscription += OnGiftedSubscription;
    _client.OnLeftChannel += OnLeftChannel;
    _client.OnNewSubscriber += OnNewSubscriber;
    _client.OnReSubscriber += OnReSubscriber;
    _client.OnUnaccountedFor += OnUnaccountedFor;

    // Retrieve list of channels to connect to
    var channels = (await _channelsRepository.GetConnected()).ToList();

    // Create bot instances for each channel
    foreach (var channel in channels)
    {
      CreateInstance(channel);
    }

    var credentials = new ConnectionCredentials(_configuration.GetSection("Twitch")["Username"], _configuration.GetSection("Twitch")["IrcToken"],
                                                capabilities: new Capabilities(false));

    _client.Initialize(credentials, channels.Select(channel => channel.NormalizedName).ToList());
    _client.AddChatCommandIdentifier('\\');
  }

  public void Connect()
  {
    _client.Connect();
  }

  private void CreateInstance(Channel channel)
  {
    using var scope = _serviceProvider.CreateScope();
    var instance = scope.ServiceProvider.GetService<BotInstance>()!;
    instance.Initialize(channel);
    _instances.Add(channel.NormalizedName, instance);
  }

  public void Disconnect()
  {
    _client.OnMessageReceived -= OnMessageReceived;
    _client.OnJoinedChannel -= OnJoinedChannel;
    _client.OnConnected -= OnConnected;
    _client.OnFailureToReceiveJoinConfirmation -= OnFailureToReceiveJoinConfirmation;
    _client.OnChatCommandReceived -= OnChatCommandReceived;
    _client.OnCommunitySubscription -= OnCommunitySubscription;
    _client.OnConnectionError -= OnConnectionError;
    _client.OnDisconnected -= OnDisconnected;
    _client.OnError -= OnError;
    _client.OnGiftedSubscription -= OnGiftedSubscription;
    _client.OnLeftChannel -= OnLeftChannel;
    _client.OnNewSubscriber -= OnNewSubscriber;
    _client.OnReSubscriber -= OnReSubscriber;
    _client.OnUnaccountedFor -= OnUnaccountedFor;

    try
    {
      _client.Disconnect();
    }
    catch (Exception ex)
    {
      Console.Write(string.Empty);
      // client might not be connected anyway
    }
  }

  private async void OnMessageReceived(object? sender, OnMessageReceivedArgs e)
  {
  }

  private async void OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
  {
    await _instances[e.Command.ChatMessage.Channel].HandleCommand(e.Command);
  }

  // Client successfully connected to Twitch
  private void OnConnected(object? sender, OnConnectedArgs e)
  {
    _logger.LogInformation("Pyrewatcher connected to Twitch");
  }

  // Client successfully disconnected from Twitch
  private void OnDisconnected(object? sender, OnDisconnectedEventArgs e)
  {
    _logger.LogInformation("Pyrewatcher disconnected from Twitch");
  }

  // Client failed to connect to Twitch
  private void OnConnectionError(object? sender, OnConnectionErrorArgs e)
  {
    _logger.LogError("Pyrewatcher failed to connect to Twitch: {Message}", e.Error.Message);
    _client.Reconnect();
  }

  // Client successfully connected to a channel
  private void OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
  {
    _logger.LogInformation("Pyrewatcher connected to channel {Channel}", e.Channel);
  }

  // Client successfully disconnected from a channel
  private void OnLeftChannel(object? sender, OnLeftChannelArgs e)
  {
    _logger.LogInformation("Pyrewatcher disconnected from channel {Channel}", e.Channel);
  }

  // Client failed to connect to a channel
  private void OnFailureToReceiveJoinConfirmation(object? sender, OnFailureToReceiveJoinConfirmationArgs e)
  {
    _logger.LogInformation("Pyrewatcher failed to connect to channel {Channel}: {Error}", e.Exception.Channel, e.Exception.Details);
    // TODO: Reconnect to channel
  }

  private void OnCommunitySubscription(object? sender, OnCommunitySubscriptionArgs e)
  {
    _logger.LogInformation("Community subscription");
    // TODO: Handle it
  }

  // Unknown, probably an exception inside the client
  private void OnError(object? sender, OnErrorEventArgs e)
  {
    _logger.LogError(e.Exception, "OnError - An error occurred: {Message}", e.Exception.Message);
    // TODO: Reconnect to Twitch
  }

  private void OnGiftedSubscription(object? sender, OnGiftedSubscriptionArgs e)
  {
    // var action = new Dictionary<string, string>
    // {
    //   {"msg-id", e.GiftedSubscription.MsgId},
    //   {"broadcaster", e.Channel},
    //   {"user-id", e.GiftedSubscription.UserId},
    //   {"display-name", e.GiftedSubscription.DisplayName},
    //   {"msg-param-sub-plan", e.GiftedSubscription.MsgParamSubPlanName},
    //   {"msg-param-recipient-id", e.GiftedSubscription.MsgParamRecipientId},
    //   {"msg-param-recipient-display-name", e.GiftedSubscription.MsgParamRecipientDisplayName}
    // };
    //
    // await _actionHandler.HandleActionAsync(action);
  }

  private void OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
  {
    // var action = new Dictionary<string, string>
    // {
    //   {"msg-id", e.Subscriber.MsgId},
    //   {"broadcaster", e.Channel},
    //   {"user-id", e.Subscriber.UserId},
    //   {"display-name", e.Subscriber.DisplayName},
    //   {"msg-param-sub-plan", e.Subscriber.SubscriptionPlanName}
    // };
    //
    // await _actionHandler.HandleActionAsync(action);
  }

  private void OnReSubscriber(object? sender, OnReSubscriberArgs e)
  {
    // var action = new Dictionaryonary<string, string>
    // {
    //   {"msg-id", e.ReSubscriber.MsgId},
    //   {"broadcaster", e.Channel},
    //   {"user-id", e.ReSubscriber.UserId},
    //   {"display-name", e.ReSubscriber.DisplayName},
    //   {"msg-param-sub-plan", e.ReSubscriber.SubscriptionPlanName}
    // };
    //
    // await _actionHandler.HandleActionAsync(action);
  }

  private void OnUnaccountedFor(object? sender, OnUnaccountedForArgs e)
  {
    _logger.LogInformation("Unhandled IRC message: {message}", e.RawIRC);
  }
}
