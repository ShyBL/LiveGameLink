using NUnit.Framework;
using UnityEngine;
using LiveGameLink.Core;

namespace LiveGameLink.Tests
{
    public class LiveGameLinkConfigTests
    {
        LiveGameLinkConfig _cfg;
        [SetUp]    public void Setup()    { _cfg = ScriptableObject.CreateInstance<LiveGameLinkConfig>(); }
        [TearDown] public void Teardown() { Object.DestroyImmediate(_cfg); }

        [Test] public void EmptyConfigFailsValidation()
        {
            Assert.IsFalse(_cfg.IsConfigured(out var err));
            Assert.IsNotNull(err);
        }

        [Test] public void MissingProjectIdFails()
        {
            _cfg.extensionClientId = "abc";
            Assert.IsFalse(_cfg.IsConfigured(out var err));
            StringAssert.Contains("Project ID", err);
        }

        [Test] public void MissingClientIdFails()
        {
            _cfg.ugsProjectId = "p1";
            Assert.IsFalse(_cfg.IsConfigured(out var err));
            StringAssert.Contains("Client ID", err);
        }

        [Test] public void FullyConfiguredPasses()
        {
            _cfg.ugsProjectId      = "abc-123";
            _cfg.extensionClientId = "xyz";
            Assert.IsTrue(_cfg.IsConfigured(out var err));
            Assert.IsNull(err);
        }

        [Test] public void OnValidateForcesProtocolVersion()
        {
            _cfg.protocolVersion = "1.0.0";
            var m = typeof(LiveGameLinkConfig).GetMethod("OnValidate",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            m.Invoke(_cfg, null);
            Assert.AreEqual(Protocol.ProtocolVersion.Current, _cfg.protocolVersion);
        }
    }
}
