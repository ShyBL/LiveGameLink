# Changelog

## 4.0.0-preview.1

- Initial UGS variant. Parallel to v3 (Cloudflare); same Protocol + Extension; backend layer swaps to UGS Cloud Code.
- C# .NET 9 Cloud Code module using TwitchLib.Extension NuGet.
- Cloud Save persists overlay state (key `twitch_overlay_state`).
- UGS Environment Variables hold the Twitch Extension Secret.
- TwitchLib.Unity integrated for PubSub viewer actions + EventSub follows/subs/redemptions.
- Anonymous UGS sign-in (SignInAnonymouslyAsync) — no separate account system.
- Stateless service-account token for overlay GetState.
