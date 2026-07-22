using Microsoft.Extensions.Logging;
using Unity.Services.CloudCode.Core;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudSave.Apis;
using TwitchLib.Extension;
using TwitchLib.Extension.Models;

namespace LiveGameLink.CloudCode;

/// Per spec, two endpoints:
///   Send     - called by Unity game (UGS Bearer). Writes state to Cloud Save, forwards to Twitch PubSub.
///   GetState - called by overlay (UGS stateless token, read-only). Returns latest state.
public class TwitchBroadcast
{
    const string CloudSaveKey = "twitch_overlay_state";

    readonly ILogger<TwitchBroadcast> _log;
    readonly IGameApiClient _gameApi;

    public TwitchBroadcast(ILogger<TwitchBroadcast> log, IGameApiClient gameApi)
    {
        _log = log;
        _gameApi = gameApi;
    }

    // ── Send ─────────────────────────────────────────────────────────────────
    [CloudCodeFunction("Send")]
    public async Task<SendResult> Send(IExecutionContext ctx, string channelId, string payload)
    {
        if (string.IsNullOrWhiteSpace(channelId))
            return new SendResult(false, "missing_channel");

        // Policy: same shape as Cloudflare worker.
        var policy = Policy.Enforce(payload);
        if (!policy.Ok)
            return new SendResult(false, policy.Code.ToString(), policy.Detail);

        // Cloud Save state cache (key is project-level; we scope by channel via a per-channel sub-key).
        try
        {
            await _gameApi.CloudSaveData.SetItemAsync(ctx, ctx.AccessToken,
                ctx.ProjectId, ctx.PlayerId,
                new Unity.Services.CloudSave.Model.SetItemBody(CloudSaveKey + ":" + channelId, payload));
        }
        catch (Exception ex) { _log.LogWarning(ex, "CloudSave write failed"); /* non-fatal */ }

        // 1 msg/sec/channel coalescer.
        string? toSend = RateLimiter.ConsumeOrQueue(channelId, payload);
        if (toSend is null)
        {
            // Within window - rely on the next call (or a scheduled flush) to drain.
            return new SendResult(true, "coalesced");
        }

        // Twitch PubSub via TwitchLib.Extension.
        try
        {
            var ext = LoadExtensionFromEnv(ctx);
            await ext.SendExtensionPubSubMessageAsync(channelId, toSend, new[] { "broadcast" });
            return new SendResult(true, "sent");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Twitch PubSub send failed");
            return new SendResult(false, "twitch_send_failed");
        }
    }

    // ── GetState ─────────────────────────────────────────────────────────────
    [CloudCodeFunction("GetState")]
    public async Task<GetStateResult> GetState(IExecutionContext ctx, string channelId)
    {
        if (string.IsNullOrWhiteSpace(channelId))
            return new GetStateResult(null, "missing_channel");

        try
        {
            var items = await _gameApi.CloudSaveData.GetItemsAsync(ctx, ctx.AccessToken,
                ctx.ProjectId, ctx.PlayerId,
                new List<string> { CloudSaveKey + ":" + channelId });
            var first = items?.Results?.FirstOrDefault();
            return new GetStateResult(first?.Value?.ToString(), first is null ? "empty" : "ok");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CloudSave read failed");
            return new GetStateResult(null, "read_failed");
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    /// TwitchLib.Extension takes Client ID + Owner ID + base64 secret.
    /// All three come from UGS Environment Variables on the project dashboard.
    static TwitchExtension LoadExtensionFromEnv(IExecutionContext ctx)
    {
        string clientId = ctx.Environment["TWITCH_EXTENSION_CLIENT_ID"]
            ?? throw new Exception("TWITCH_EXTENSION_CLIENT_ID env var missing");
        string ownerId  = ctx.Environment["TWITCH_EXTENSION_OWNER_ID"]
            ?? throw new Exception("TWITCH_EXTENSION_OWNER_ID env var missing");
        string secret   = ctx.Environment["TWITCH_EXTENSION_SECRET"]
            ?? throw new Exception("TWITCH_EXTENSION_SECRET env var missing");
        // Spec: TwitchLib.Extension. Construct with the owner/client/secret triple.
        return new TwitchExtension(clientId, ownerId, secret);
    }
}

public record SendResult(bool Ok, string Status, string? Detail = null);
public record GetStateResult(string? State, string Status);
