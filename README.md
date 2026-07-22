# Live Game Link 4 (UGS)

Unity Gaming Services variant of Live Game Link. **Zero servers to deploy or maintain.** All backend lives inside UGS:

| Layer | Technology | Hosted |
|-------|------------|--------|
| Game | Twitch Unity SDK + UGS SDK + TwitchLib.Unity | Local / shipped game |
| Backend | UGS Cloud Code module (C# .NET 9 + TwitchLib.Extension NuGet) | Unity Gaming Services |
| State persistence | UGS Cloud Save | Unity Gaming Services |
| Secrets | UGS Environment Variables | Unity Dashboard |
| Overlay | HTML/CSS/JS + Twitch Extension Helper v1.js | Twitch CDN |

## Why v4 vs v3?

- **v3** uses Cloudflare Workers + KV. Cheap at scale ($0.05/M req past free tier), but you have to know `wrangler`, KV, and TypeScript.
- **v4** uses UGS. Free tier covers 50k MAU. You never leave Unity; the Deployment window pushes the Cloud Code module from the same IDE you're already in.
- See [docs/why-ugs-instead-of-cloudflare.md](docs/why-ugs-instead-of-cloudflare.md) for the trade-off matrix.

## Install

1. Unity 2022.3+: **Window → Package Manager → + → Install from disk** → `com.livegamelink4.unity/package.json`.
2. **Window → Live Game Link → Integration Wizard** auto-opens.
3. Wizard installs Twitch Unity SDK + TwitchLib.Unity, links your UGS project, deploys the Cloud Code module, walks you through Twitch dev portal.

## Architecture

```
Unity Game
  ├─ TwitchSDK (auth + channelId)
  ├─ TwitchLib.Unity (PubSub for viewer actions, EventSub for follows/subs/redemptions)
  └─ UGS SDK (SignInAnonymouslyAsync → automatic Bearer token)
       └─ CloudCodeService.CallModuleEndpointAsync("TwitchBroadcast/Send", payload)
              │
              ▼
Cloud Code module  (.NET 9, in UGS)
  ├─ Reads TWITCH_EXTENSION_SECRET from Environment Variables
  ├─ Writes state to Cloud Save (key: twitch_overlay_state)
  ├─ TwitchLib.Extension.SendExtensionPubSubMessageAsync()
  └─ In-module queue: 1 msg/sec rate limit, coalesce bursts
              │
              ▼
Twitch PubSub → all live viewers
              │
              ▼
Overlay (Twitch Extension Helper v1.js)
  ├─ onAuthorized() → fetch GET TwitchBroadcast/GetState (stateless token, read-only)
  └─ listen("broadcast") → live updates
```
