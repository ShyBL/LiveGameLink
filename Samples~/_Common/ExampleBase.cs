using System.Threading.Tasks;
using UnityEngine;
using LiveGameLink;

namespace LiveGameLink.Samples
{
    /// Inherit and override the hooks you care about. Auto-initializes the runtime on Start().
    public abstract class ExampleBase : MonoBehaviour
    {
        protected async void Start()
        {
            if (!LiveGameLinkRuntime.IsInitialized)
                if (!await LiveGameLinkRuntime.Initialize()) return;

            if (LiveGameLinkRuntime.Twitch != null)
                LiveGameLinkRuntime.Twitch.OnConnected += HandleConnected;
            if (LiveGameLinkRuntime.Twitch?.IsReady ?? false)
                HandleConnected(LiveGameLinkRuntime.Twitch.ChannelId);
        }

        void OnDestroy()
        {
            if (LiveGameLinkRuntime.Twitch != null)
                LiveGameLinkRuntime.Twitch.OnConnected -= HandleConnected;
        }

        void HandleConnected(string channelId)
        {
            if (LiveGameLinkRuntime.Events != null)
            {
                LiveGameLinkRuntime.Events.OnFollow      += OnFollow;
                LiveGameLinkRuntime.Events.OnSubscribe   += OnSubscribe;
                LiveGameLinkRuntime.Events.OnRedemption  += OnRedemption;
                LiveGameLinkRuntime.Events.OnViewerAction+= OnViewerAction;
            }
            OnTwitchReady(channelId);
        }

        protected virtual void OnTwitchReady(string channelId) {}
        protected virtual void OnFollow(string userLogin) {}
        protected virtual void OnSubscribe(string userLogin, string tier) {}
        protected virtual void OnRedemption(string userLogin, string rewardTitle, int cost) {}
        protected virtual void OnViewerAction(string userId, string elementId) {}

        protected Task Send(Protocol.Broadcast b)
            => LiveGameLinkRuntime.Broadcast?.Send(b) ?? Task.CompletedTask;
    }
}
