using LiveGameLink.Protocol;

namespace LiveGameLink.Samples
{
    public class ChannelPointsExample : ExampleBase
    {
        protected override void OnRedemption(string userLogin, string rewardTitle, int cost)
        {
            _ = Send(new Broadcast { type = MessageType.Toast, toast = userLogin + " redeemed " + rewardTitle });
        }
    }
}
