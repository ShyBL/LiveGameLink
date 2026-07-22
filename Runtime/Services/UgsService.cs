using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using LiveGameLink.Core;

namespace LiveGameLink.Services
{
    /// Wraps UGS init + anonymous auth. Per spec: SignInAnonymouslyAsync attaches a Bearer token
    /// to every subsequent CloudCodeService call automatically. UGS async/await resumes on
    /// Unity's main thread via SynchronizationContext - no dispatcher required.
    public sealed class UgsService : ILiveGameLinkService
    {
        readonly LiveGameLinkConfig _config;

        public bool IsReady { get; private set; }
        public string PlayerId { get; private set; }

        public event Action OnSignedIn;
        public event Action<string> OnError;

        public UgsService(LiveGameLinkConfig config) { _config = config; }

        public async Task InitializeAsync()
        {
            try
            {
                var options = new InitializationOptions();
                if (!string.IsNullOrEmpty(_config.ugsEnvironment))
                    options.SetEnvironmentName(_config.ugsEnvironment);
                await UnityServices.InitializeAsync(options);
            }
            catch (Exception ex) { OnError?.Invoke("UGS init failed: " + ex.Message); return; }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                try { await AuthenticationService.Instance.SignInAnonymouslyAsync(); }
                catch (Exception ex) { OnError?.Invoke("Anonymous sign-in failed: " + ex.Message); return; }
            }

            PlayerId = AuthenticationService.Instance.PlayerId;
            IsReady  = true;
            OnSignedIn?.Invoke();
        }

        public void Dispose()
        {
            try { AuthenticationService.Instance.SignOut(); } catch { }
            IsReady = false;
        }
    }
}
