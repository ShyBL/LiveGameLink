using System.Collections.Generic;
using UnityEngine;
using LiveGameLink.Protocol;

namespace LiveGameLink.Core
{
    /// Non-sensitive UGS-variant config. Safe to commit. Secrets (Extension Secret) live in
    /// UGS Environment Variables on the dashboard, never in Unity.
    [CreateAssetMenu(fileName = "LiveGameLinkConfig", menuName = "Live Game Link/Config", order = 0)]
    public class LiveGameLinkConfig : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/LiveGameLink/Resources/LiveGameLinkConfig.asset";

        [Header("Unity Gaming Services")]
        [Tooltip("UGS Project ID. From Unity Dashboard → your project → Settings → General.")]
        public string ugsProjectId;

        [Tooltip("UGS Environment name. Default 'production'.")]
        public string ugsEnvironment = "production";

        [Tooltip("Cloud Code module slug. Default 'TwitchBroadcast'.")]
        public string cloudCodeModule = "TwitchBroadcast";

        [Header("Twitch")]
        [Tooltip("Your extension's Client ID. From dev.twitch.tv. Non-sensitive.")]
        public string extensionClientId;

        [Header("Protocol")]
        public string protocolVersion = ProtocolVersion.Current;

        [Header("Declared Manifest (Twitch-policy safeguard)")]
        [Tooltip("Element IDs your game broadcasts. Cloud Code module rejects undeclared IDs.")]
        public List<string> declaredElementIds = new List<string>();

        [Header("Behavior")]
        public bool openWizardOnFirstImport = true;
        public bool autoCheckForUpdates     = true;

        void OnValidate() { protocolVersion = ProtocolVersion.Current; }

        public bool IsConfigured(out string error)
        {
            if (string.IsNullOrWhiteSpace(ugsProjectId))      { error = "UGS Project ID is empty.";      return false; }
            if (string.IsNullOrWhiteSpace(extensionClientId)) { error = "Extension Client ID is empty."; return false; }
            error = null;
            return true;
        }
    }
}
