#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using LiveGameLink.Core;

namespace LiveGameLink.Editor.Settings
{
    public static class LiveGameLinkSettingsProvider
    {
        const string EditorPrefsPrefix = "LGL4.";

        public const string SecretExtensionSecret = "ExtensionSecret";
        public const string SecretUgsServiceToken = "UgsServiceToken";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider("Project/Live Game Link", SettingsScope.Project)
            {
                label = "Live Game Link",
                guiHandler = _ =>
                {
                    var cfg = LoadOrCreate();
                    EditorGUILayout.LabelField("Unity Gaming Services", EditorStyles.boldLabel);
                    cfg.ugsProjectId    = EditorGUILayout.TextField("Project ID",      cfg.ugsProjectId);
                    cfg.ugsEnvironment  = EditorGUILayout.TextField("Environment",     cfg.ugsEnvironment);
                    cfg.cloudCodeModule = EditorGUILayout.TextField("Cloud Code Module", cfg.cloudCodeModule);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Twitch", EditorStyles.boldLabel);
                    cfg.extensionClientId = EditorGUILayout.TextField("Extension Client ID", cfg.extensionClientId);

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Secrets (EditorPrefs)", EditorStyles.boldLabel);
                    DrawSecret("Extension Secret (base64)",       SecretExtensionSecret);
                    DrawSecret("UGS Service-Account Token (read)", SecretUgsServiceToken);
                    EditorGUILayout.HelpBox(
                        "Secrets are stored in EditorPrefs on your machine. The Extension Secret goes into UGS Dashboard → Environment Variables → TWITCH_EXTENSION_SECRET. The stateless service token is pasted into the extension's config.html.",
                        MessageType.Info);

                    EditorGUILayout.Space();
                    cfg.openWizardOnFirstImport = EditorGUILayout.Toggle("Open wizard on first import", cfg.openWizardOnFirstImport);
                    cfg.autoCheckForUpdates     = EditorGUILayout.Toggle("Check for updates",            cfg.autoCheckForUpdates);

                    if (GUI.changed) { EditorUtility.SetDirty(cfg); AssetDatabase.SaveAssets(); }

                    EditorGUILayout.Space();
                    if (GUILayout.Button("Open Integration Wizard")) IntegrationWizard.Open();
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[]{"Twitch","Live","Game","Link","UGS","Cloud Code","Stream"})
            };
        }

        public static LiveGameLinkConfig LoadOrCreate()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<LiveGameLinkConfig>(LiveGameLinkConfig.DefaultAssetPath);
            if (cfg != null) return cfg;
            string dir = Path.GetDirectoryName(LiveGameLinkConfig.DefaultAssetPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            cfg = ScriptableObject.CreateInstance<LiveGameLinkConfig>();
            AssetDatabase.CreateAsset(cfg, LiveGameLinkConfig.DefaultAssetPath);
            AssetDatabase.SaveAssets();
            return cfg;
        }

        public static string GetSecret(string key)    => EditorPrefs.GetString(EditorPrefsPrefix + key, "");
        public static void  SetSecret(string key, string v)
        {
            if (string.IsNullOrEmpty(v)) EditorPrefs.DeleteKey(EditorPrefsPrefix + key);
            else                          EditorPrefs.SetString(EditorPrefsPrefix + key, v);
        }

        static void DrawSecret(string label, string key)
        {
            string cur = GetSecret(key);
            string nv  = EditorGUILayout.PasswordField(label, cur);
            if (nv != cur) SetSecret(key, nv);
        }
    }
}
#endif
