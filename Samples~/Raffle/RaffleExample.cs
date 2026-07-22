using LiveGameLink.UI;

namespace LiveGameLink.Samples
{
    public class RaffleExample : ExampleBase
    {
        protected override void OnTwitchReady(string channelId)
        {
            var b = new TwitchUIBuilder()
                .Button("raffle_enter", "Enter the raffle")
                .Stat("raffle_count", "Entrants", 0)
                .Build();
            _ = Send(b);
        }
    }
}
