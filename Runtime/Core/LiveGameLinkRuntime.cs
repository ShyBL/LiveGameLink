using System;
using System.Threading.Tasks;
using UnityEngine;
using LiveGameLink.Services;

namespace LiveGameLink
{
    /// Static facade per spec. Customers call LiveGameLinkRuntime.Initialize() then read
    /// .Twitch / .Ugs / .Broadcast / .Events. A hidden host GameObject hosts the
    /// SynchronizationContext - the customer never instantiates a MonoBehaviour for the runtime.
    public static class LiveGameLinkRuntime
    {
        public static Core.LiveGameLinkConfig Config { get; private set; }
        public static TwitchService    Twitch    { get; private set; }
        public static UgsService       Ugs       { get; private set; }
        public static BroadcastService Broadcast { get; private set; }
        public static EventsService    Events    { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static event Action OnReady;
        public static event Action<string> OnError;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BootstrapHost()
        {
            var go = new GameObject("[LGL] Host");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<LiveGameLinkHost>();
        }

        public static async Task<bool> Initialize(Core.LiveGameLinkConfig config = null)
        {
            if (IsInitialized) return true;
            Config = config ?? Resources.Load<Core.LiveGameLinkConfig>("LiveGameLinkConfig");
            if (Config == null)              { Fail("LiveGameLinkConfig not found at Resources/LiveGameLinkConfig."); return false; }
            if (!Config.IsConfigured(out var err)) { Fail(err); return false; }

            // Order matters: UGS init first (Bearer token attaches to subsequent Cloud Code calls).
            Ugs       = new UgsService(Config);
            Twitch    = new TwitchService(Config);
            Broadcast = new BroadcastService(Config, Twitch, Ugs);
            Events    = new EventsService(Twitch);

            try
            {
                await Ugs.InitializeAsync().ConfigureAwait(true);
                await Twitch.InitializeAsync().ConfigureAwait(true);
                await Broadcast.InitializeAsync().ConfigureAwait(true);
                await Events.InitializeAsync().ConfigureAwait(true);
            }
            catch (Exception ex) { Fail("Initialize failed: " + ex.Message); return false; }

            IsInitialized = true;
            OnReady?.Invoke();
            return true;
        }

        public static void Shutdown()
        {
            Events?.Dispose();
            Broadcast?.Dispose();
            Twitch?.Dispose();
            Ugs?.Dispose();
            IsInitialized = false;
        }

        static void Fail(string m) { Debug.LogError("[LGL] " + m); OnError?.Invoke(m); }

        sealed class LiveGameLinkHost : MonoBehaviour
        {
            void OnApplicationQuit() => Shutdown();
        }
    }
}
