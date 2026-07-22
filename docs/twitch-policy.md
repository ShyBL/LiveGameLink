# Twitch Extension Policy Compliance (v4)

Same safeguards as v3 — the difference is just where they live. The full Twitch guidelines: [dev.twitch.tv/docs/extensions/guidelines-and-policies/](https://dev.twitch.tv/docs/extensions/guidelines-and-policies/).

## Why per-developer extensions (recap from v3)

A shared extension would let one bad customer's payload trigger takedown for everyone. So every developer uses their own Twitch extension, their own UGS project, their own keys.

## Where each safeguard lives in v4

| Twitch rule | File |
|-------------|------|
| No unauthorized external links in any rendered text | [Backend~/ugs-cloud-code/Policy.cs](../Backend~/ugs-cloud-code/Policy.cs), [Extension~/src/policy.js](../Extension~/src/policy.js), [Runtime/Core/PayloadValidator.cs](../Runtime/Core/PayloadValidator.cs) |
| No misleading / impersonating UI | Renderer uses `.textContent` only ([Extension~/src/viewer.js](../Extension~/src/viewer.js)); element kinds whitelisted to `button \| progress \| stat \| toast` |
| No dynamic content that wasn't in review | Declared-element manifest in [Backend~/ugs-cloud-code/Schema.cs](../Backend~/ugs-cloud-code/Schema.cs) `DeclaredElementIds` AND [Extension~/manifest.js](../Extension~/manifest.js). Both reject unknown IDs. |
| CSP / iframe ancestry | Hardcoded CSP meta in `Extension~/*.html` pin `frame-ancestors https://*.twitch.tv` |
| Reasonable payload sizes | 4 KB cap, 64-element cap in `Policy.cs` |
| Rate compliance | 1 msg/sec/channel via [Backend~/ugs-cloud-code/RateLimiter.cs](../Backend~/ugs-cloud-code/RateLimiter.cs) in-module queue |
| Broadcaster auditability | UGS Dashboard → Cloud Code → Logs shows every `TwitchBroadcast/Send` invocation |

## How to demonstrate compliance to a Twitch reviewer

Submission notes:

> "All UI elements are pre-declared in the extension's `manifest.js` and in the Cloud Code module's `Schema.cs.DeclaredElementIds`. Both layers reject undeclared IDs at runtime. External links (`http://` / `https://`) are blocked in element labels and toast text by `Policy.cs` (server) and `policy.js` (client). The renderer uses `.textContent` only — no `innerHTML`, no inline scripts. CSP locks `frame-ancestors` to `*.twitch.tv`."

That's verifiable from the uploaded extension zip; the reviewer can read `manifest.js`, `policy.js`, `viewer.js` directly.
