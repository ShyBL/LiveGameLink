# Live Game Link 4 — Extension (UGS variant)

Three Twitch extension surfaces, same shape as v3 but the overlay bootstraps state from UGS Cloud Code (not Cloudflare KV).

- `video_overlay.html` — viewer overlay. Renders UI manifests, listens for Twitch PubSub broadcasts.
- `config.html` — broadcaster's one-time setup. Stores UGS project id + stateless token in extension config.
- `live-config.html` — broadcaster dashboard. Connection status only in v4; audit log lives in UGS Dashboard → Logs.

## Upload to Twitch

1. Zip this folder. dev.twitch.tv → Your Extensions → Manage → Files → upload.
2. **Asset Hosting**:
   - Video Overlay → `video_overlay.html`
   - Broadcaster Live Dashboard → `live-config.html`
   - Configuration → `config.html`
3. **Capabilities → CSP & Whitelisted Hosts**: add `services.api.unity.com` (so the overlay can fetch GetState).

## Why is there a stateless token in the config?

UGS Cloud Code endpoints normally require an authenticated player token. The overlay isn't a player — it's a viewer's browser. The standard pattern is a **service-account stateless token** with read-only permissions limited to `TwitchBroadcast/GetState`. Generate from UGS Dashboard → Service Accounts.
