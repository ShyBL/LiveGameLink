using System.Collections.Generic;
using LiveGameLink.Protocol;

namespace LiveGameLink.UI
{
    public sealed class TwitchUIBuilder
    {
        readonly List<UIElement> _elements = new List<UIElement>();
        public UITheme Theme { get; }
        public TwitchUIBuilder(UITheme theme = null) { Theme = theme; }

        public TwitchUIBuilder Button(string id, string label)
            { _elements.Add(new UIElement { id = id, kind = ElementKind.Button, label = label }); return this; }
        public TwitchUIBuilder Progress(string id, string label, float value, float min, float max)
            { _elements.Add(new UIElement { id = id, kind = ElementKind.Progress, label = label, value = value, min = min, max = max }); return this; }
        public TwitchUIBuilder Stat(string id, string label, float value)
            { _elements.Add(new UIElement { id = id, kind = ElementKind.Stat, label = label, value = value }); return this; }

        public Broadcast Build(string type = MessageType.UiManifest)
        {
            var b = new Broadcast { type = type };
            b.elements.AddRange(_elements);
            return b;
        }
        public void Clear() => _elements.Clear();
        public int Count => _elements.Count;
    }
}
