# UGS Cloud Code Deploy

The Integration Wizard's "Deploy" step launches one of these flows. Here's the manual version.

## Prerequisites

- Unity 2022.3+ with the Twitch Unity SDK + TwitchLib.Unity installed
- A UGS project linked under **Edit → Project Settings → Services**
- .NET 9 SDK installed locally (only needed if you use the CLI; the in-Unity Deployment Window handles compilation internally)
- (For CLI path) `ugs` CLI: download from [services.docs.unity.com/guides/ugs-cli](https://services.docs.unity.com/guides/ugs-cli/)

## Option A — UGS Deployment Window (recommended)

1. **Window → Package Manager** → install `com.unity.services.deployment` (Deployment package).
2. **Services → Deployment**.
3. The window scans your project for deployable items. `com.livegamelink4.unity/Backend~/ugs-cloud-code/TwitchBroadcast` appears.
4. Right-click → **Deploy** (or check it and click Deploy at the bottom).
5. Unity compiles the C# module and pushes it to your linked UGS project under the active environment.

## Option B — UGS CLI

```sh
cd com.livegamelink4.unity/Backend~/ugs-cloud-code
ugs login                     # browser-based UGS auth (one-time)
ugs cloud-code deploy . \
    --project-id <UGS_PROJECT_ID> \
    --environment-name production
```

## Environment Variables

Set on UGS Dashboard → Cloud Code → **Environment Variables**:

| Name | Value |
|------|-------|
| `TWITCH_EXTENSION_CLIENT_ID` | your extension's Client ID (from dev.twitch.tv) |
| `TWITCH_EXTENSION_OWNER_ID`  | your numeric Twitch user ID (the account that owns the extension) |
| `TWITCH_EXTENSION_SECRET`    | base64 from dev.twitch.tv (sensitive) |

The module reads these at runtime via `ctx.Environment[...]`. Without all three, `TwitchBroadcast/Send` throws.

## Stateless service-account token for the extension overlay

The overlay's bootstrap fetch (`TwitchBroadcast/GetState`) needs a UGS token. Player tokens aren't applicable in a viewer's browser, so generate a **service account**:

1. UGS Dashboard → **Service Accounts** → Create.
2. Permissions: **Cloud Code** → **Read** only. Limit to the `TwitchBroadcast` module if the dashboard supports module-level scopes.
3. Copy the bearer token.
4. Open the extension's `config.html` (Twitch's local rig or live config), paste the token + project id + environment name.

The token is stored in extension configuration (broadcaster segment). Twitch encrypts the segment server-side.

## Verify

```sh
# Trigger a broadcast from Unity Play mode → check logs
ugs cloud-code logs --module TwitchBroadcast --tail
```

You should see `TwitchBroadcast/Send` invocations with the channel id you logged in as.

## Free-tier math

UGS Cloud Code: **50,000 MAU** free. Each unique signed-in player counts once/month toward MAU. For a broadcasting game, MAU = number of unique streamers who used your plugin in that month.

Above 50k MAU, UGS pricing tiers apply; check [services.docs.unity.com/cloud-code/pricing](https://services.docs.unity.com/cloud-code/pricing/) for current rates.
