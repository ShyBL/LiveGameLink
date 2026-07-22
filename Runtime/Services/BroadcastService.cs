using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudCode;
using LiveGameLink.Core;
using LiveGameLink.Protocol;

namespace LiveGameLink.Services
{
    /// Per spec: single method Send(payload) -> CloudCodeService.Instance.CallModuleEndpointAsync.
    /// async/await resumes on the main thread; no coroutine, no dispatcher.
    /// Skips silently if TwitchService.IsReady is false.
    public sealed class BroadcastService : ILiveGameLinkService
    {
        readonly LiveGameLinkConfig _config;
        readonly TwitchService _twitch;
        readonly UgsService _ugs;

        public bool IsReady => _twitch != null && _ugs != null && _twitch.IsReady && _ugs.IsReady;
        public event Action<string> OnError;

        public BroadcastService(LiveGameLinkConfig config, TwitchService twitch, UgsService ugs)
        {
            _config = config; _twitch = twitch; _ugs = ugs;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task Send(Broadcast payload)
        {
            if (!IsReady) return;
            if (payload == null) return;

            payload.protocolVersion = ProtocolVersion.Current;
            payload.ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            string json = JsonUtility.ToJson(payload);
            int bytes = Encoding.UTF8.GetByteCount(json);
            var v = PayloadValidator.Validate(payload, _config.declaredElementIds, bytes);
            if (v != PayloadValidator.Result.Ok) { OnError?.Invoke("Payload rejected client-side: " + v); return; }

            try
            {
                // The Cloud Code module endpoint is `{module}/Send` per spec.
                var args = new Dictionary<string, object>
                {
                    { "channelId", _twitch.ChannelId },
                    { "payload",   json }
                };
                await CloudCodeService.Instance.CallModuleEndpointAsync(
                    _config.cloudCodeModule, "Send", args).ConfigureAwait(true);
            }
            catch (CloudCodeException ex) { OnError?.Invoke("CloudCode " + ex.ErrorCode + ": " + ex.Message); }
            catch (Exception ex)           { OnError?.Invoke("Send failed: " + ex.Message); }
        }

        public void Dispose() {}
    }
}
