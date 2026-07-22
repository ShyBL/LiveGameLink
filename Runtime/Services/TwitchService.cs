using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using LiveGameLink.Core;

#if LGL_TWITCH_SDK
using Twitch;
#endif

namespace LiveGameLink.Services
{
    /// Wraps the official Twitch Unity SDK. Per spec:
    ///   - Poll Twitch.API.GetAuthState().MaybeResult until AuthStatus.LoggedIn
    ///   - Read channelId once via Twitch.API.GetMyUserInfo()
    ///   - Token persistence, refresh, and Device Code Flow UI handled entirely by the SDK
    public sealed class TwitchService : ILiveGameLinkService
    {
        readonly LiveGameLinkConfig _config;

        public bool IsReady       { get; private set; }
        public string ChannelId   { get; private set; }
        public string DisplayName { get; private set; }

        public event Action<string> OnConnected;
        public event Action<string> OnError;

        public TwitchService(LiveGameLinkConfig config) { _config = config; }

        public async Task InitializeAsync()
        {
#if LGL_TWITCH_SDK
            try { await Twitch.API.Init(_config.extensionClientId).AsTask().ConfigureAwait(true); }
            catch (Exception ex) { OnError?.Invoke("SDK init failed: " + ex.Message); return; }

            // Per spec: poll until AuthStatus.LoggedIn. Stay off the main-thread Update by yielding.
            while (Twitch.API.GetAuthState().Result != AuthStatus.LoggedIn)
                await Task.Delay(500).ConfigureAwait(true);

            try
            {
                var me = await Twitch.API.GetMyUserInfo().AsTask().ConfigureAwait(true);
                ChannelId   = me?.Id;
                DisplayName = me?.DisplayName;
            }
            catch (Exception ex) { OnError?.Invoke("GetMyUserInfo failed: " + ex.Message); return; }

            if (string.IsNullOrEmpty(ChannelId)) { OnError?.Invoke("ChannelId resolved empty."); return; }

            IsReady = true;
            OnConnected?.Invoke(ChannelId);
#else
            await Task.Yield();
            Debug.LogWarning("[LGL] Twitch Unity SDK missing (LGL_TWITCH_SDK undefined). Run the Integration Wizard.");
#endif
        }

        public Task BeginLoginAsync(string[] scopes)
        {
#if LGL_TWITCH_SDK
            return Twitch.API.Login(scopes).WaitForCompletionAsync();
#else
            return Task.CompletedTask;
#endif
        }

        public void Dispose()
        {
#if LGL_TWITCH_SDK
            try { Twitch.API.Dispose(); } catch { }
#endif
            IsReady = false;
        }
    }
}
