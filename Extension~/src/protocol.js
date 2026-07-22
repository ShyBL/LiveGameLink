// Mirror of Runtime/Protocol/Messages.cs and Backend~/ugs-cloud-code/Schema.cs.
window.LGL = window.LGL || {};
LGL.PROTOCOL_VERSION = '3.0.0';
LGL.KINDS = ['button', 'progress', 'stat', 'toast'];
LGL.MSG = {
  UI_MANIFEST:   'UI_MANIFEST',
  STATE_PATCH:   'STATE_PATCH',
  TOAST:         'TOAST',
  CLEAR_UI:      'CLEAR_UI',
  VIEWER_ACTION: 'VIEWER_ACTION'
};

LGL.protocolMajor = v => (v && typeof v === 'string') ? v.split('.')[0] : null;

LGL.parseEnvelope = function (raw) {
  try {
    const o = (typeof raw === 'string') ? JSON.parse(raw) : raw;
    if (!o || typeof o.type !== 'string') return null;
    if (LGL.protocolMajor(o.protocolVersion) !== LGL.protocolMajor(LGL.PROTOCOL_VERSION)) return null;
    return o;
  } catch { return null; }
};
