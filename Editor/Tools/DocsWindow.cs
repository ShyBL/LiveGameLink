#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LiveGameLink.Editor.Tools
{
    public class DocsWindow : EditorWindow
    {
        [MenuItem("Window/Live Game Link/Documentation")]
        public static void Open() => GetWindow<DocsWindow>(false, "LGL Docs", true);

        Vector2 _scroll;
        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Live Game Link 4 (UGS)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Quickstart:\n" +
                "  1. Window → Live Game Link → Integration Wizard\n" +
                "  2. Install Twitch SDK + TwitchLib.Unity\n" +
                "  3. Link UGS project, set env vars on dashboard\n" +
                "  4. Declare your UI element IDs in the Manifest step\n" +
                "  5. Deploy the Cloud Code module via Deployment Window or `ugs` CLI\n" +
                "  6. Upload Extension~/ to dev.twitch.tv\n",
                MessageType.Info);
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
