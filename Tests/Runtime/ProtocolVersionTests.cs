using NUnit.Framework;
using LiveGameLink.Protocol;

namespace LiveGameLink.Tests
{
    public class ProtocolVersionTests
    {
        [Test] public void CurrentIs3() => Assert.AreEqual("3.0.0", ProtocolVersion.Current);
        [Test] public void AcceptsMatch()         => Assert.IsTrue(ProtocolVersion.IsCompatible("3.0.0"));
        [Test] public void AcceptsMinorDrift()    => Assert.IsTrue(ProtocolVersion.IsCompatible("3.1.5"));
        [Test] public void RejectsMajorMismatch() => Assert.IsFalse(ProtocolVersion.IsCompatible("2.9.9"));
        [Test] public void RejectsEmpty()         => Assert.IsFalse(ProtocolVersion.IsCompatible(""));
        [Test] public void RejectsNull()          => Assert.IsFalse(ProtocolVersion.IsCompatible(null));
    }
}
