#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using LiveGameLink.Core;
using LiveGameLink.Editor.Settings;
using Debug = UnityEngine.Debug;

namespace LiveGameLink.Editor
{
    /// 8-step Integration Wizard for the UGS variant. Auto-opens on first import.
    public class IntegrationWizard : EditorWindow
    {
        const string PrefsSeenKey = "LGL4.WizardSeen";

        enum Step { Welcome, Sdk, Ugs, TwitchExt, EnvVars, Manifest, Module, Scene, Done }
        [SerializeField] Step _step = Step.Welcome;
        [SerializeField] bool[] _completed = new bool[9];

        Vector2 _scroll;
        AddRequest _addRequest;
        string _sdkStatus;
        string _manifestEntry = "";

        static readonly Color Purple = new Color(0.57f, 0.27f, 0.99f);

        [MenuItem("Window/Live Game Link/Integration Wizard")]
        public static void Open()
        {
            var w = GetWindow<IntegrationWizard>(true, "Live Game Link 4 — Setup", true);
            w.minSize = new Vector2(720, 580);
            w.Show();
        }

        [InitializeOnLoadMethod]
        static void MaybeAutoOpen()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorPrefs.GetBool(PrefsSeenKey, false)) return;
                var cfg = LiveGameLinkSettingsProvider.LoadOrCreate();
                if (!cfg.openWizardOnFirstImport) return;
                EditorPrefs.SetBool(PrefsSeenKey, true);
                Open();
            };
        }

        void OnGUI()
        {
            DrawHeader();
            DrawStepStrip();
            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            switch (_step)
            {
                case Step.Welcome:   DrawWelcome();   break;
                case Step.Sdk:       DrawSdk();       break;
                case Step.Ugs:       DrawUgs();       break;
                case Step.TwitchExt: DrawTwitchExt(); break;
                case Step.EnvVars:   DrawEnvVars();   break;
                case Step.Manifest:  DrawManifest();  break;
                case Step.Module:    DrawModule();    break;
                case Step.Scene:     DrawScene();     break;
                case Step.Done:      DrawDone();      break;
            }
            EditorGUILayout.EndScrollView();
            DrawNav();
        }

        void DrawHeader()
        {
            var r = EditorGUILayout.GetControlRect(false, 36f);
            EditorGUI.DrawRect(r, Purple * new Color(1,1,1,0.4f));
            GUI.Label(new Rect(r.x + 12, r.y + 8, r.width, 22),
                "Live Game Link 4 — UGS Setup",
                new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 });
        }

        void DrawStepStrip()
        {
            EditorGUILayout.BeginHorizontal();
            string[] names = { "Welcome", "SDK", "UGS", "Twitch Ext", "Env Vars", "Manifest", "Deploy", "Scene", "Done" };
            for (int i = 0; i < names.Length; i++)
            {
                bool reachable = IsReachable((Step)i);
                GUI.enabled = reachable;
                if (GUILayout.Button(names[i], EditorStyles.miniButton)) _step = (Step)i;
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        bool IsReachable(Step s)
        {
            for (int i = 0; i < (int)s; i++) if (!_completed[i]) return false;
            return true;
        }

        void DrawNav()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = (int)_step > 0;
            if (GUILayout.Button("Back", GUILayout.Width(80))) _step = (Step)((int)_step - 1);
            GUI.enabled = (int)_step < 8 && _completed[(int)_step];
            if (GUILayout.Button("Next →", GUILayout.Width(80))) _step = (Step)((int)_step + 1);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        // ── Welcome ──────────────────────────────────────────────────────────
        void DrawWelcome()
        {
            EditorGUILayout.HelpBox(
                "Live Game Link 4 (UGS variant) turns your Unity game into a Twitch overlay extension - " +
                "fully serverless via Unity Gaming Services.\n\n" +
                "You'll need (~15 minutes):\n" +
                "  • A Twitch account\n" +
                "  • A Unity ID with UGS access (free up to 50k MAU)\n" +
                "  • .NET 9 SDK installed locally (for the Cloud Code module deploy)\n\n" +
                "This wizard:\n" +
                "  1. Installs Twitch Unity SDK + TwitchLib.Unity\n" +
                "  2. Links your UGS project\n" +
                "  3. Walks you through Twitch extension creation\n" +
                "  4. Sets UGS Environment Variables for the Extension Secret\n" +
                "  5. Declares your UI element manifest (Twitch policy safeguard)\n" +
                "  6. Deploys the Cloud Code module\n" +
                "  7. Drops a runtime bootstrap into your scene",
                MessageType.Info);
            if (GUILayout.Button("Begin", GUILayout.Height(28))) { _completed[(int)Step.Welcome] = true; _step = Step.Sdk; }
        }

        // ── SDK Bootstrap ────────────────────────────────────────────────────
        void DrawSdk()
        {
            EditorGUILayout.LabelField("Twitch Unity SDK + TwitchLib.Unity", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Twitch Unity SDK handles Device Code auth + channelId. TwitchLib.Unity handles PubSub " +
                "viewer-action stream and EventSub follow/sub/redemption events.",
                MessageType.Info);

            bool sdk =
#if LGL_TWITCH_SDK
                true;
#else
                false;
#endif
            bool lib =
#if LGL_TWITCHLIB
                true;
#else
                false;
#endif
            EditorGUILayout.LabelField("Twitch Unity SDK detected", sdk ? "Yes" : "No");
            EditorGUILayout.LabelField("TwitchLib.Unity detected",  lib ? "Yes" : "No");

            if (!sdk)
            {
                if (GUILayout.Button("Install Twitch Unity SDK (com.twitch.sdk)"))
                {
                    _addRequest = Client.Add("com.twitch.sdk");
                    EditorApplication.update += PollAddRequest;
                    _sdkStatus = "Installing Twitch SDK...";
                }
            }
            if (!lib)
            {
                if (GUILayout.Button("Install TwitchLib.Unity (com.twitchlib.unity)"))
                {
                    _addRequest = Client.Add("com.twitchlib.unity");
                    EditorApplication.update += PollAddRequest;
                    _sdkStatus = "Installing TwitchLib.Unity...";
                }
            }
            if (sdk && lib) _completed[(int)Step.Sdk] = true;
            if (!string.IsNullOrEmpty(_sdkStatus)) EditorGUILayout.LabelField("Status", _sdkStatus);
        }

        void PollAddRequest()
        {
            if (_addRequest == null) { EditorApplication.update -= PollAddRequest; return; }
            if (!_addRequest.IsCompleted) return;
            EditorApplication.update -= PollAddRequest;
            _sdkStatus = _addRequest.Status == StatusCode.Success ? "Installed." : ("Failed: " + _addRequest.Error?.message);
            _addRequest = null;
            AssetDatabase.Refresh();
            Repaint();
        }

        // ── UGS link ─────────────────────────────────────────────────────────
        void DrawUgs()
        {
            EditorGUILayout.LabelField("Link Unity Gaming Services", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Open Edit → Project Settings → Services.\n" +
                "2. Sign in with your Unity ID and link a UGS project.\n" +
                "3. Copy the Project ID and Environment name and paste them here.",
                MessageType.Info);

            var cfg = LiveGameLinkSettingsProvider.LoadOrCreate();
            cfg.ugsProjectId   = EditorGUILayout.TextField("UGS Project ID", cfg.ugsProjectId);
            cfg.ugsEnvironment = EditorGUILayout.TextField("Environment",    cfg.ugsEnvironment);

            if (GUILayout.Button("Open Project Settings → Services"))
                SettingsService.OpenProjectSettings("Project/Services");

            bool ok = !string.IsNullOrWhiteSpace(cfg.ugsProjectId);
            using (new EditorGUI.DisabledScope(!ok))
                if (GUILayout.Button("Save"))
                {
                    EditorUtility.SetDirty(cfg);
                    AssetDatabase.SaveAssets();
                    _completed[(int)Step.Ugs] = true;
                }
        }

        // ── Twitch Extension ─────────────────────────────────────────────────
        void DrawTwitchExt()
        {
            EditorGUILayout.LabelField("Twitch Extension", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Create your extension at dev.twitch.tv. You'll get a Client ID (non-sensitive) and " +
                "an Extension Secret (sensitive - goes into UGS Environment Variables next step).",
                MessageType.Info);

            if (GUILayout.Button("Open dev.twitch.tv → Create Extension"))
                Application.OpenURL("https://dev.twitch.tv/console/extensions/create");

            var cfg = LiveGameLinkSettingsProvider.LoadOrCreate();
            cfg.extensionClientId = EditorGUILayout.TextField("Extension Client ID", cfg.extensionClientId);

            string secret = LiveGameLinkSettingsProvider.GetSecret(LiveGameLinkSettingsProvider.SecretExtensionSecret);
            string ns     = EditorGUILayout.PasswordField("Extension Secret (base64)", secret);
            if (ns != secret) LiveGameLinkSettingsProvider.SetSecret(LiveGameLinkSettingsProvider.SecretExtensionSecret, ns);

            bool ok = !string.IsNullOrWhiteSpace(cfg.extensionClientId) && !string.IsNullOrWhiteSpace(ns);
            using (new EditorGUI.DisabledScope(!ok))
                if (GUILayout.Button("Save"))
                {
                    EditorUtility.SetDirty(cfg);
                    AssetDatabase.SaveAssets();
                    _completed[(int)Step.TwitchExt] = true;
                }
        }

        // ── Env Vars upload (manual paste) ───────────────────────────────────
        void DrawEnvVars()
        {
            EditorGUILayout.LabelField("UGS Environment Variables", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Open UGS Dashboard → your project → Cloud Code → Environment Variables → Add. Set:\n\n" +
                "  TWITCH_EXTENSION_CLIENT_ID = <your client id>\n" +
                "  TWITCH_EXTENSION_OWNER_ID  = <your numeric Twitch user id>\n" +
                "  TWITCH_EXTENSION_SECRET    = <base64 secret>\n\n" +
                "The Cloud Code module reads these at runtime. They never enter Unity binaries or git.",
                MessageType.Info);

            if (GUILayout.Button("Copy values to clipboard"))
            {
                var cfg = LiveGameLinkSettingsProvider.LoadOrCreate();
                string secret = LiveGameLinkSettingsProvider.GetSecret(LiveGameLinkSettingsProvider.SecretExtensionSecret);
                string text =
                    "TWITCH_EXTENSION_CLIENT_ID=" + cfg.extensionClientId + "\n" +
                    "TWITCH_EXTENSION_OWNER_ID=<paste your twitch user id>\n" +
                    "TWITCH_EXTENSION_SECRET=" + secret + "\n";
                EditorGUIUtility.systemCopyBuffer = text;
                EditorUtility.DisplayDialog("Copied", "Paste into UGS Dashboard → Cloud Code → Environment Variables.", "OK");
            }

            if (GUILayout.Button("Open UGS Dashboard"))
                Application.OpenURL("https://cloud.unity.com/home/organizations");

            if (GUILayout.Button("I've set the env vars on the dashboard"))
                _completed[(int)Step.EnvVars] = true;
        }

        // ── Manifest ─────────────────────────────────────────────────────────
        void DrawManifest()
        {
            EditorGUILayout.LabelField("Declared Element Manifest", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Declare every UI element ID your game will broadcast. The Cloud Code module AND the " +
                "extension viewer.js reject undeclared IDs. Twitch policy safeguard.",
                MessageType.Info);

            var cfg = LiveGameLinkSettingsProvider.LoadOrCreate();
            for (int i = 0; i < cfg.declaredElementIds.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                cfg.declaredElementIds[i] = EditorGUILayout.TextField(cfg.declaredElementIds[i]);
                if (GUILayout.Button("✕", GUILayout.Width(24))) { cfg.declaredElementIds.RemoveAt(i); GUIUtility.ExitGUI(); }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            _manifestEntry = EditorGUILayout.TextField(_manifestEntry);
            if (GUILayout.Button("Add", GUILayout.Width(60)) && !string.IsNullOrWhiteSpace(_manifestEntry))
            {
                cfg.declaredElementIds.Add(_manifestEntry.Trim());
                _manifestEntry = "";
                EditorUtility.SetDirty(cfg);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (GUILayout.Button("Write manifest to Extension~/manifest.js + Cloud Code Schema.cs"))
            {
                WriteManifestFiles(cfg);
                _completed[(int)Step.Manifest] = true;
            }
        }

        static void WriteManifestFiles(LiveGameLinkConfig cfg)
        {
            string pkgRoot = FindPackageRoot();
            if (string.IsNullOrEmpty(pkgRoot)) { Debug.LogError("[LGL] Cannot locate package root."); return; }

            string ids = string.Join(",", cfg.declaredElementIds.ConvertAll(s => "'" + s.Replace("'", "") + "'"));
            string idsCs = string.Join(", ", cfg.declaredElementIds.ConvertAll(s => "\"" + s.Replace("\"", "") + "\""));

            string manifestJs =
                "window.LGL_MANIFEST = { protocolVersion: '" + Protocol.ProtocolVersion.Current + "', declaredElementIds: [" + ids + "] };\n";
            File.WriteAllText(Path.Combine(pkgRoot, "Extension~", "manifest.js"), manifestJs);

            string schemaPath = Path.Combine(pkgRoot, "Backend~", "ugs-cloud-code", "Schema.cs");
            if (File.Exists(schemaPath))
            {
                string text = File.ReadAllText(schemaPath);
                string marker = "public static readonly HashSet<string> DeclaredElementIds = new()\n    {";
                int i = text.IndexOf(marker, StringComparison.Ordinal);
                if (i >= 0)
                {
                    int j = text.IndexOf("};", i, StringComparison.Ordinal);
                    if (j > i)
                    {
                        string rebuilt = text.Substring(0, i + marker.Length) + "\n        " + idsCs + "\n    " + text.Substring(j);
                        File.WriteAllText(schemaPath, rebuilt);
                    }
                }
            }
            Debug.Log("[LGL] Manifest written. Redeploy the Cloud Code module + re-upload the extension.");
        }

        static string FindPackageRoot()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(LiveGameLinkConfig.DefaultAssetPath);
            if (info != null) return info.resolvedPath;
            string repoRoot = Directory.GetParent(Application.dataPath).FullName;
            foreach (var d in Directory.GetDirectories(repoRoot, "com.livegamelink4.unity", SearchOption.AllDirectories))
                return d;
            return null;
        }

        // ── Deploy Cloud Code module ─────────────────────────────────────────
        void DrawModule()
        {
            EditorGUILayout.LabelField("Deploy Cloud Code Module", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Two options:\n\n" +
                "A. Window → Deployment (UGS Deployment package) → right-click Backend~/ugs-cloud-code → Deploy\n\n" +
                "B. Terminal:\n" +
                "    cd com.livegamelink4.unity/Backend~/ugs-cloud-code\n" +
                "    ugs login\n" +
                "    ugs cloud-code deploy . --project-id <ID> --environment-name <ENV>\n",
                MessageType.Info);

            if (GUILayout.Button("Open Deployment Window"))
            {
                EditorApplication.ExecuteMenuItem("Services/Deployment");
            }

            if (GUILayout.Button("Open terminal at Backend~/ugs-cloud-code"))
            {
                string pkg = FindPackageRoot();
                if (string.IsNullOrEmpty(pkg)) return;
                string path = Path.Combine(pkg, "Backend~", "ugs-cloud-code");
                try { Process.Start(new ProcessStartInfo { FileName = "cmd.exe", Arguments = "/k cd /d \"" + path + "\"", UseShellExecute = true }); }
                catch (Exception ex) { Debug.LogError("[LGL] Couldn't open terminal: " + ex.Message); }
            }

            if (GUILayout.Button("Module deployed - continue"))
                _completed[(int)Step.Module] = true;
        }

        // ── Scene Setup ──────────────────────────────────────────────────────
        void DrawScene()
        {
            EditorGUILayout.LabelField("Scene Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "LiveGameLinkRuntime is static. A hidden host GameObject is created at BeforeSceneLoad. " +
                "Add a kick-off script that calls LiveGameLinkRuntime.Initialize() from your scene, OR " +
                "drop one of the Samples~ ExampleBase subclasses.",
                MessageType.Info);

            if (GUILayout.Button("Create LiveGameLinkBootstrap GameObject"))
            {
                var go = new GameObject("LiveGameLinkBootstrap");
                Undo.RegisterCreatedObjectUndo(go, "Create LGL Bootstrap");
                Selection.activeGameObject = go;
                _completed[(int)Step.Scene] = true;
            }
        }

        void DrawDone()
        {
            EditorGUILayout.LabelField("All set!", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Next:\n" +
                "  • Generate a UGS service-account token (Dashboard → Service Accounts) with read-only Cloud Code permissions\n" +
                "  • Upload Extension~/ as a zip to dev.twitch.tv; set Asset Hosting URLs\n" +
                "  • Open the extension's config.html in Twitch's local rig; paste Project ID + Env + service-account token\n" +
                "  • Run your scene and check UGS Dashboard → Cloud Code → Logs",
                MessageType.Info);
            if (GUILayout.Button("Open Twitch Extensions Dashboard"))
                Application.OpenURL("https://dev.twitch.tv/console/extensions");
            if (GUILayout.Button("Open UGS Dashboard"))
                Application.OpenURL("https://cloud.unity.com/home/organizations");
            if (GUILayout.Button("Close")) Close();
        }
    }
}
#endif
