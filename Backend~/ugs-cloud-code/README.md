# Live Game Link v4 — UGS Cloud Code Module

C# .NET 9 module deployed to Unity Gaming Services. Replaces the v3 Cloudflare Worker.

## Endpoints

| Endpoint | Auth | Use |
|----------|------|-----|
| `TwitchBroadcast/Send` | UGS Bearer (game player token) | Game pushes UI state. Writes to Cloud Save, forwards to Twitch PubSub. |
| `TwitchBroadcast/GetState` | UGS stateless token | Overlay bootstrap on load. Reads from Cloud Save. |

## Environment Variables (set in UGS Dashboard)

- `TWITCH_EXTENSION_CLIENT_ID` — your extension's Client ID
- `TWITCH_EXTENSION_OWNER_ID` — your Twitch numeric user ID
- `TWITCH_EXTENSION_SECRET` — base64 from dev.twitch.tv (sensitive)

## Deploy

### Option A — UGS Deployment Window (inside Unity)

1. **Window → Deployment** (opens the Deployment package window — installed via the wizard).
2. Right-click `com.livegamelink4.unity/Backend~/ugs-cloud-code/` → **Deploy**.
3. Unity pushes the module to your linked UGS project under the current environment.

### Option B — UGS CLI

```sh
cd com.livegamelink4.unity/Backend~/ugs-cloud-code
ugs login
ugs cloud-code deploy . --project-id <UGS_PROJECT_ID> --environment-name production
```

## Policy enforced

1. ≤ 4 KB payload, ≤ 64 UI elements
2. Element IDs match `^[a-zA-Z0-9_-]{1,64}$`
3. Only `button | progress | stat | toast` kinds
4. Only **declared** element IDs (see `Schema.cs` `DeclaredElementIds`)
5. No `http://` / `https://` in any label or toast
6. 1 msg/sec/channel coalescer

## Rate limits

UGS platform: 200 req/sec per IP. Twitch PubSub: 1 msg/sec/channel. Our `RateLimiter` keeps us under both.
