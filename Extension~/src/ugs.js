// UGS Cloud Code call from the browser. Per spec: GetState is auth'd by a stateless
// service-account token (read-only). The token + project id come from extension
// configuration (set in config.html).
(() => {
  'use strict';
  LGL.ugs = {
    /// Returns the latest broadcast body (raw JSON string) for the given channel,
    /// or null if no state is cached yet.
    async getState(channelId) {
      const cfg = readConfig();
      if (!cfg || !cfg.projectId || !cfg.token) return null;

      const url = `https://services.api.unity.com/cloud-code/v1/projects/${cfg.projectId}/modules/TwitchBroadcast/GetState`;
      try {
        const r = await fetch(url, {
          method:  'POST',
          headers: {
            'Authorization': 'Bearer ' + cfg.token,
            'Content-Type':  'application/json',
            'Unity-Environment-Name': cfg.env || 'production'
          },
          body: JSON.stringify({ params: { channelId } })
        });
        if (!r.ok) return null;
        const data = await r.json();
        // CloudCode wraps the return value in { output: <GetStateResult> }
        const out = data && (data.output || data);
        return (out && out.State) || null;
      } catch { return null; }
    }
  };

  function readConfig() {
    try {
      const seg = window.Twitch && Twitch.ext && Twitch.ext.configuration && Twitch.ext.configuration.broadcaster;
      if (seg && seg.content) return JSON.parse(seg.content);
    } catch {}
    return null;
  }
})();
