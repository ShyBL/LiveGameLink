# UGS vs. Cloudflare — trade-off matrix

Both v3 (Cloudflare Workers) and v4 (UGS Cloud Code) exist. They share the protocol, the Unity Runtime API surface, the extension front-end, and the policy safeguards. They differ in **where the backend lives**.

| Concern | v3 — Cloudflare | v4 — UGS Cloud Code |
|---------|-----------------|---------------------|
| **Where you deploy from** | Terminal (`wrangler`) | Inside Unity (Deployment Window) or `ugs` CLI |
| **Language you write** | TypeScript | C# .NET 9 (same language as your game) |
| **Free tier** | 100,000 requests/day | 50,000 monthly active users |
| **Cost past free** | $5/month for 10M req, then $0.50/M | UGS tier pricing — usually higher per-request |
| **Vendor lock-in** | Low — TS/JS in an industry-standard runtime | Higher — tied to UGS APIs (Cloud Code, Cloud Save) |
| **State persistence** | Workers KV (eventually consistent, ~50ms global propagation) | Cloud Save (strong-consistent per player) |
| **Secret storage** | `wrangler secret put` (Cloudflare account) | UGS Environment Variables (Unity Dashboard) |
| **Cold start** | ~0 ms (V8 isolates) | ~100-300 ms (managed .NET runtime) |
| **Auth model** | Custom bearer (`GAME_API_KEY`) we issue | UGS Bearer (automatic via `SignInAnonymouslyAsync`) |
| **Overlay-side bootstrap** | Public `/state` endpoint, public read | UGS stateless service-account token in extension config |
| **Local development** | `wrangler dev` (instant) | UGS doesn't have offline dev — deploy + read logs |
| **Quota dimension** | Requests | MAU |

## Picking between them

**Pick v3 (Cloudflare) if:**
- You expect heavy per-request usage (constantly updating HUDs, many viewers) and want to budget on request count.
- You prefer not to commit to a Unity-vendor backend.
- You already know `wrangler` / Cloudflare.
- You want fast deploys with `wrangler dev` for iteration.

**Pick v4 (UGS) if:**
- You're already using UGS for other game services (Cloud Save, Lobbies, etc.) and want one bill.
- You prefer one language across game + backend (C# both sides).
- Your MAU count is small (< 50k) and you don't want to think about per-request pricing.
- You like staying inside the Unity Editor end-to-end.

## What's identical between v3 and v4

- The protocol (`schema.json`, version 3.0.0)
- The Unity Runtime API (`LiveGameLinkRuntime.Broadcast.Send(...)`)
- The Extension HTML/CSS/JS structure (only `ugs.js` differs from v3's direct `/state` fetch)
- Policy enforcement: declared-element manifest, `button|progress|stat|toast` whitelist, no-external-links, 1 msg/sec rate limit
- Sample base class (`ExampleBase`)
- Test layout and asmdefs

This means **switching is cheap**. If you start on v4 and outgrow the MAU tier, copying the same project to v3 takes about an hour: delete `Backend~/ugs-cloud-code/`, drop in `Backend~/cloudflare/` from v3, change `BroadcastService` to do `HttpClient.PostAsync` instead of `CloudCodeService.CallModuleEndpointAsync`, redeploy. The game-facing API doesn't change.
