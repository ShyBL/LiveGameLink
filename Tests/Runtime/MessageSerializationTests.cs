using NUnit.Framework;
using UnityEngine;
using LiveGameLink.Protocol;

namespace LiveGameLink.Tests
{
    /// JsonUtility round-trips: any drift between Messages.cs and what the Cloud Code module
    /// (Schema.cs) deserializes will surface as a silently dropped broadcast.
    public class MessageSerializationTests
    {
        [Test] public void EnvelopeRoundTrip()
        {
            var e = new Envelope { type = "FOO", ts = 1234 };
            var json = JsonUtility.ToJson(e);
            var back = JsonUtility.FromJson<Envelope>(json);
            Assert.AreEqual("FOO", back.type);
            Assert.AreEqual(1234, back.ts);
            Assert.AreEqual(ProtocolVersion.Current, back.protocolVersion);
        }

        [Test] public void BroadcastWithElementsRoundTrip()
        {
            var b = new Broadcast { type = MessageType.UiManifest, ts = 99 };
            b.elements.Add(new UIElement { id = "hp", kind = ElementKind.Progress, value = 50, max = 100 });
            var json = JsonUtility.ToJson(b);
            var back = JsonUtility.FromJson<Broadcast>(json);
            Assert.AreEqual(1, back.elements.Count);
            Assert.AreEqual("hp", back.elements[0].id);
            Assert.AreEqual(50f, back.elements[0].value);
        }

        [Test] public void ViewerActionRoundTrip()
        {
            var v = new ViewerAction { type = MessageType.ViewerAction, action = "button_click", elementId = "click_me", userId = "u1" };
            var json = JsonUtility.ToJson(v);
            var back = JsonUtility.FromJson<ViewerAction>(json);
            Assert.AreEqual("button_click", back.action);
            Assert.AreEqual("click_me",     back.elementId);
            Assert.AreEqual("u1",           back.userId);
        }

        [Test] public void JsonContainsProtocolVersion()
        {
            var json = JsonUtility.ToJson(new Broadcast { type = MessageType.Toast, toast = "hi" });
            StringAssert.Contains("\"protocolVersion\":\"3.0.0\"", json);
        }
    }
}
