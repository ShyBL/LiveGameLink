/* global Twitch, LGL */
(() => {
  'use strict';
  const statusEl = document.getElementById('status');
  let isBroadcaster = false;
  let lastSeen = 0;

  Twitch.ext.onAuthorized(a => {
    isBroadcaster = a.role === 'broadcaster';
    statusEl.textContent = isBroadcaster ? 'Ready' : 'Read-only (not broadcaster)';
  });

  Twitch.ext.listen('broadcast', (_t, _c, raw) => {
    const env = LGL.parseEnvelope(raw);
    if (!env) return;
    lastSeen = Date.now();
    statusEl.textContent = 'Game connected (' + env.type + ')';
  });

  setInterval(() => {
    if (Date.now() - lastSeen > 10_000)
      statusEl.textContent = isBroadcaster ? 'Waiting for game...' : 'Read-only';
  }, 5_000);
})();
