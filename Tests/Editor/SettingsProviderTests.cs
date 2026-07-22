using NUnit.Framework;
using UnityEditor;
using LiveGameLink.Editor.Settings;

namespace LiveGameLink.Tests.Editor
{
    public class SettingsProviderTests
    {
        const string Key = "UnitTest_Secret";

        [TearDown] public void Teardown() { EditorPrefs.DeleteKey("LGL4." + Key); }

        [Test] public void SetSecretRoundTrips()
        {
            LiveGameLinkSettingsProvider.SetSecret(Key, "abc123");
            Assert.AreEqual("abc123", LiveGameLinkSettingsProvider.GetSecret(Key));
        }

        [Test] public void EmptyValueDeletesKey()
        {
            LiveGameLinkSettingsProvider.SetSecret(Key, "abc");
            LiveGameLinkSettingsProvider.SetSecret(Key, "");
            Assert.AreEqual("", LiveGameLinkSettingsProvider.GetSecret(Key));
        }

        [Test] public void UnknownKeyReturnsEmpty()
        {
            Assert.AreEqual("", LiveGameLinkSettingsProvider.GetSecret("zzz_unknown"));
        }
    }
}
