using UnityEngine;
using LiveGameLink.UI;

namespace LiveGameLink.Samples
{
    public class InteractionsExample : ExampleBase
    {
        protected override void OnTwitchReady(string channelId)
        {
            var b = new TwitchUIBuilder().Button("click_me", "Click me!").Build();
            _ = Send(b);
        }

        protected override void OnViewerAction(string userId, string elementId)
        {
            Debug.Log($"[Interactions] {userId} clicked {elementId}");
        }
    }
}
