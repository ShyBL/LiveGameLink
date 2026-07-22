/* global Twitch */
(() => {
  'use strict';
  let auth = null;

  Twitch.ext.onAuthorized(a => {
    auth = a;
    try {
      const seg = Twitch.ext.configuration && Twitch.ext.configuration.broadcaster;
      if (seg && seg.content) {
        const c = JSON.parse(seg.content);
        document.getElementById('project').value = c.projectId || '';
        document.getElementById('env').value     = c.env       || 'production';
        document.getElementById('token').value   = c.token     || '';
      }
    } catch {}
  });

  document.getElementById('save').addEventListener('click', () => {
    const projectId = document.getElementById('project').value.trim();
    const env       = document.getElementById('env').value.trim() || 'production';
    const token     = document.getElementById('token').value.trim();
    if (!projectId || !token) {
      document.getElementById('msg').textContent = 'Project ID and token are required.';
      return;
    }
    Twitch.ext.configuration.set('broadcaster', '3.0.0', JSON.stringify({ projectId, env, token }));
    document.getElementById('msg').textContent = 'Saved.';
  });
})();
