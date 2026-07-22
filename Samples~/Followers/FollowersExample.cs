using LiveGameLink.Protocol;

namespace LiveGameLink.Samples
{
    public class FollowersExample : ExampleBase
    {
        protected override void OnFollow(string userLogin)
        {
            _ = Send(new Broadcast { type = MessageType.Toast, toast = "New follower: " + userLogin });
        }
    }
}
