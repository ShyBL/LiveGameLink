using UnityEngine;
using LiveGameLink.UI;

namespace LiveGameLink.Samples
{
    public class ShopExample : ExampleBase
    {
        [SerializeField] string[] _items = { "shop_buy_sword", "shop_buy_shield", "shop_buy_potion" };

        protected override void OnTwitchReady(string channelId)
        {
            var b = new TwitchUIBuilder();
            foreach (var id in _items) b.Button(id, id.Replace("shop_buy_", "").Replace("_", " "));
            _ = Send(b.Build());
        }

        protected override void OnViewerAction(string userId, string elementId)
        {
            Debug.Log($"[Shop] {userId} clicked {elementId}");
        }
    }
}
