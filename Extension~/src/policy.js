// Client-side enforcer. Final line of defense after the Cloud Code module.
(() => {
  'use strict';
  const declared = new Set((window.LGL_MANIFEST && window.LGL_MANIFEST.declaredElementIds) || []);
  const ID_RE = /^[a-zA-Z0-9_-]{1,64}$/;

  LGL.policyOk = function (msg) {
    if (!msg || !LGL.MSG[msg.type]) return false;
    if (msg.elements) {
      if (!Array.isArray(msg.elements) || msg.elements.length > 64) return false;
      for (const el of msg.elements) {
        if (!el || !ID_RE.test(el.id || '')) return false;
        if (LGL.KINDS.indexOf(el.kind) < 0)  return false;
        if (declared.size > 0 && !declared.has(el.id)) return false;
        if (typeof el.label === 'string' && /https?:\/\//i.test(el.label)) return false;
      }
    }
    if (typeof msg.toast === 'string') {
      if (msg.toast.length > 200) return false;
      if (/https?:\/\//i.test(msg.toast)) return false;
    }
    return true;
  };
})();
