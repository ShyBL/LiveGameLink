using System.Collections.Concurrent;

namespace LiveGameLink.CloudCode;

/// Per-channel in-module queue per spec. Twitch's hard limit is 1 PubSub message/sec/channel.
/// We coalesce - keep latest payload, drop stale during the 1-sec window, flush when it expires.
/// Cloud Code modules run in a serverless container so static state survives across calls within
/// the same warm instance. Across instances we accept duplicate sends in the worst case; Twitch's
/// own rate-limit absorbs them.
public sealed class RateLimiter
{
    static readonly ConcurrentDictionary<string, long> LastSentMs = new();
    static readonly ConcurrentDictionary<string, PendingItem> Pending = new();
    public const int MinIntervalMs = 1000;

    public sealed record PendingItem(string Body, DateTimeOffset Enqueued);

    /// Returns the body to send NOW (or null if we should wait). If we should wait, the caller
    /// stores the body via TryEnqueue and the next call within the window coalesces.
    public static string? ConsumeOrQueue(string channelId, string body)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long last = LastSentMs.GetValueOrDefault(channelId, 0);
        if (now - last >= MinIntervalMs)
        {
            LastSentMs[channelId] = now;
            // If something was pending, it gets superseded by this body.
            Pending.TryRemove(channelId, out _);
            return body;
        }
        // Within the window: replace pending with the latest body.
        Pending[channelId] = new PendingItem(body, DateTimeOffset.UtcNow);
        return null;
    }

    /// Caller may invoke this after sleeping the remaining-window time to fetch+clear a pending.
    public static string? DrainPending(string channelId)
    {
        if (Pending.TryRemove(channelId, out var p)) { LastSentMs[channelId] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); return p.Body; }
        return null;
    }
}
