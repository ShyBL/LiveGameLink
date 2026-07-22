/* global Twitch, LGL */
(() => {
  'use strict';
  const root      = document.getElementById('lgl-root');
  const toastRoot = document.getElementById('lgl-toasts');
  let auth = null;

  // ── toast queue (max 3, drop oldest with indicator) ─────────────────────
  const TOASTS = [];
  function pushToast(text) {
    while (TOASTS.length >= 3) {
      const d = TOASTS.shift();
      d.el.classList.add('lgl-toast--dropped');
      setTimeout(() => d.el.remove(), 200);
    }
    const el = document.createElement('div');
    el.className = 'lgl-toast';
    el.textContent = text;
    toastRoot.appendChild(el);
    const entry = { el };
    entry.t = setTimeout(() => {
      el.classList.add('lgl-toast--out');
      setTimeout(() => { el.remove(); const i = TOASTS.indexOf(entry); if (i >= 0) TOASTS.splice(i, 1); }, 300);
    }, 4000);
    TOASTS.push(entry);
  }

  // ── auth + bootstrap fetch (per spec) ───────────────────────────────────
  Twitch.ext.onAuthorized(async a => {
    auth = a;
    // Per spec: fetch Cloud Code GetState with stateless token to bootstrap late-joining viewers.
    const stateJson = await LGL.ugs.getState(auth.channelId);
    if (stateJson) {
      const env = LGL.parseEnvelope(stateJson);
      if (env && LGL.policyOk(env)) apply(env);
    }
  });

  // ── live updates via Twitch PubSub ──────────────────────────────────────
  Twitch.ext.listen('broadcast', (_t, _c, raw) => {
    const env = LGL.parseEnvelope(raw);
    if (!env) return;
    if (!LGL.policyOk(env)) return;
    apply(env);
  });

  // ── renderer (textContent only) ─────────────────────────────────────────
  function apply(b) {
    switch (b.type) {
      case LGL.MSG.UI_MANIFEST: renderManifest(b.elements || []); break;
      case LGL.MSG.STATE_PATCH: patchManifest(b.elements || []); break;
      case LGL.MSG.TOAST:       pushToast(String(b.toast || '')); break;
      case LGL.MSG.CLEAR_UI:    root.innerHTML = ''; break;
    }
  }

  function renderManifest(els) {
    root.innerHTML = '';
    for (const el of els) root.appendChild(buildElement(el));
  }
  function patchManifest(els) {
    for (const el of els) {
      const existing = root.querySelector('[data-id="' + CSS.escape(el.id) + '"]');
      const fresh = buildElement(el);
      if (existing) existing.replaceWith(fresh); else root.appendChild(fresh);
    }
  }

  function buildElement(el) {
    const n = document.createElement('div');
    n.className = 'lgl-el lgl-el--' + el.kind;
    n.setAttribute('data-id', el.id);
    switch (el.kind) {
      case 'button': {
        const b = document.createElement('button');
        b.textContent = el.label || el.id;
        b.addEventListener('click', () => sendAction(el.id));
        n.appendChild(b);
        break;
      }
      case 'progress': {
        const wrap = document.createElement('div'); wrap.className = 'lgl-bar';
        const fill = document.createElement('div'); fill.className = 'lgl-bar__fill';
        const range = Math.max(1e-6, (el.max || 1) - (el.min || 0));
        fill.style.width = (Math.max(0, Math.min(1, ((el.value || 0) - (el.min || 0)) / range)) * 100).toFixed(1) + '%';
        wrap.appendChild(fill);
        n.appendChild(label(el.label)); n.appendChild(wrap);
        break;
      }
      case 'stat': {
        n.appendChild(label(el.label));
        const v = document.createElement('span'); v.className = 'lgl-stat__value'; v.textContent = String(el.value ?? '');
        n.appendChild(v);
        break;
      }
    }
    return n;
  }

  function label(t) { const s = document.createElement('span'); s.className = 'lgl-label'; s.textContent = t || ''; return s; }

  function sendAction(elementId) {
    if (!auth) return;
    // Viewer actions go via Twitch PubSub whisper - game's TwitchLib.Unity PubSub client receives them.
    Twitch.ext.send('whisper-' + auth.channelId + '-' + auth.clientId, 'application/json', JSON.stringify({
      protocolVersion: LGL.PROTOCOL_VERSION,
      type: LGL.MSG.VIEWER_ACTION,
      ts: Date.now(),
      action: 'button_click',
      elementId,
      userId: auth.userId
    }));
  }
})();
