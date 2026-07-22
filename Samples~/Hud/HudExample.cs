using UnityEngine;
using LiveGameLink.UI;

namespace LiveGameLink.Samples
{
    public class HudExample : ExampleBase
    {
        [SerializeField] float _hp = 100f;
        [SerializeField] float _maxHp = 100f;

        protected override void OnTwitchReady(string channelId)
        {
            var b = new TwitchUIBuilder()
                .Progress("hp", "Health", _hp, 0, _maxHp)
                .Stat("score", "Score", 0)
                .Build();
            _ = Send(b);
        }
    }
}
