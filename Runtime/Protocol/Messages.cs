using System;
using System.Collections.Generic;

namespace LiveGameLink.Protocol
{
    public static class ProtocolVersion
    {
        public const string Current = "3.0.0";
        public static bool IsCompatible(string remote)
        {
            if (string.IsNullOrEmpty(remote)) return false;
            var p = remote.Split('.'); var l = Current.Split('.');
            return p.Length >= 1 && l.Length >= 1 && p[0] == l[0];
        }
    }

    public static class MessageType
    {
        public const string UiManifest   = "UI_MANIFEST";
        public const string StatePatch   = "STATE_PATCH";
        public const string Toast        = "TOAST";
        public const string ClearUi      = "CLEAR_UI";
        public const string ViewerAction = "VIEWER_ACTION";
    }

    public static class ElementKind
    {
        public const string Button   = "button";
        public const string Progress = "progress";
        public const string Stat     = "stat";
        public const string Toast    = "toast";
        public static readonly string[] All = { Button, Progress, Stat, Toast };
    }

    [Serializable]
    public class Envelope
    {
        public string protocolVersion = ProtocolVersion.Current;
        public string type;
        public long   ts;
    }

    [Serializable]
    public class Broadcast : Envelope
    {
        public List<UIElement> elements = new List<UIElement>();
        public string toast;
    }

    [Serializable]
    public class UIElement
    {
        public string id;
        public string kind;
        public string label;
        public float  value;
        public float  min;
        public float  max;
    }

    [Serializable]
    public class ViewerAction : Envelope
    {
        public string action;
        public string elementId;
        public string userId;
    }
}
