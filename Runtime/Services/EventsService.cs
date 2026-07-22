using System;
using System.Threading.Tasks;
using UnityEngine;
using LiveGameLink.Core;

#if LGL_TWITCHLIB
using TwitchLib.Unity;
using TwitchLib.PubSub.Events;
using TwitchLib.EventSub.Websockets;
#endif

namespace LiveGameLink.Services
{
    /// Per spec: TwitchLib.Unity for PubSub (viewer actions) and EventSub (follows/subs/redemptions).
    /// TwitchLib marshals all callbacks to Unity's main thread internally - no dispatcher needed.
    /// Subscriptions are wired on-demand; samples opt in via Manager.Events.OnFollow += ...
    public sealed class EventsService : ILiveGameLinkService
    {
        readonly TwitchService _twitch;

        public bool IsReady { get; private set; }

        public event Action<string> OnFollow;                       // user_login
        public event Action<string, string> OnSubscribe;             // user_login, tier
        public event Action<string, string, int> OnRedemption;       // user_login, reward_title, cost
        public event Action<string, string> OnViewerAction;          // userId, elementId

#if LGL_TWITCHLIB
        PubSub _pubsub;
        EventSubWebsocketClient _eventSub;
#endif

        public EventsService(TwitchService twitch) { _twitch = twitch; }

        public Task InitializeAsync()
        {
#if LGL_TWITCHLIB
            // PubSub for extension whisper topics (viewer-action stream).
            _pubsub = new PubSub();
            _pubsub.OnPubSubServiceConnected += (_, __) =>
            {
                // Listen on whisper-<channel>-<ext> once connected. Topic wiring happens in
                // a small helper at game startup since we need the channelId from TwitchService.
            };

            // EventSub for follows/subs/redemptions.
            _eventSub = new EventSubWebsocketClient();
            _eventSub.ChannelFollow         += (_, e) => OnFollow?.Invoke(e.Notification.Payload.Event.UserLogin);
            _eventSub.ChannelSubscribe      += (_, e) => OnSubscribe?.Invoke(e.Notification.Payload.Event.UserLogin, e.Notification.Payload.Event.Tier);
            _eventSub.ChannelPointsCustomRewardRedemptionAdd += (_, e) =>
                OnRedemption?.Invoke(e.Notification.Payload.Event.UserLogin,
                                     e.Notification.Payload.Event.Reward.Title,
                                     e.Notification.Payload.Event.Reward.Cost);
#endif
            IsReady = true;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
#if LGL_TWITCHLIB
            try { _pubsub?.Disconnect(); } catch { }
            try { _eventSub?.DisconnectAsync(); } catch { }
#endif
            IsReady = false;
        }
    }
}
