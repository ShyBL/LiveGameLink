using UnityEngine;
using LiveGameLink.UI;

namespace LiveGameLink.Samples
{
    public class VoteExample : ExampleBase
    {
        [SerializeField] string[] _options = { "vote_left", "vote_right", "vote_jump" };

        protected override void OnTwitchReady(string channelId)
        {
            var b = new TwitchUIBuilder();
            foreach (var id in _options) b.Button(id, id.Replace("vote_", ""));
            _ = Send(b.Build());
        }
    }
}
