using UnityEngine;
using LiveGameLink.UI;

namespace LiveGameLink.Samples
{
    public class CustomizationExample : ExampleBase
    {
        [SerializeField] UITheme _theme;

        protected override void OnTwitchReady(string channelId)
        {
            var b = new TwitchUIBuilder(_theme).Button("greet", "Hello, viewers").Build();
            _ = Send(b);
        }
    }
}
