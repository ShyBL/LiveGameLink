using System.Threading.Tasks;

namespace LiveGameLink.Core
{
    /// Spec verbatim: uniform lifecycle for every service hung off LiveGameLinkRuntime.
    public interface ILiveGameLinkService
    {
        bool IsReady { get; }
        Task InitializeAsync();
        void Dispose();
    }
}
