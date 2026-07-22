using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using LiveGameLink.Core;
using LiveGameLink.Protocol;

namespace LiveGameLink.Tests
{
    public class PayloadValidatorTests
    {
        static readonly HashSet<string> Declared = new HashSet<string> { "hp", "score", "shop_buy" };

        static int Bytes(Broadcast b) => Encoding.UTF8.GetByteCount(JsonUtility.ToJson(b));

        [Test] public void AcceptsValid()
        {
            var b = new Broadcast { type = MessageType.UiManifest };
            b.elements.Add(new UIElement { id = "hp", kind = ElementKind.Progress, label = "Health", value = 50, min = 0, max = 100 });
            Assert.AreEqual(PayloadValidator.Result.Ok, PayloadValidator.Validate(b, Declared, Bytes(b)));
        }

        [Test] public void RejectsUndeclaredId()
        {
            var b = new Broadcast { type = MessageType.UiManifest };
            b.elements.Add(new UIElement { id = "admin_only", kind = ElementKind.Button, label = "x" });
            Assert.AreEqual(PayloadValidator.Result.UndeclaredId, PayloadValidator.Validate(b, Declared, Bytes(b)));
        }

        [Test] public void RejectsBadKind()
        {
            var b = new Broadcast { type = MessageType.UiManifest };
            b.elements.Add(new UIElement { id = "hp", kind = "iframe", label = "x" });
            Assert.AreEqual(PayloadValidator.Result.BadKind, PayloadValidator.Validate(b, Declared, Bytes(b)));
        }

        [Test] public void RejectsBadIdChars()
        {
            var b = new Broadcast { type = MessageType.UiManifest };
            b.elements.Add(new UIElement { id = "hp;drop", kind = ElementKind.Stat });
            Assert.AreEqual(PayloadValidator.Result.BadId, PayloadValidator.Validate(b, Declared, Bytes(b)));
        }

        [Test] public void RejectsExternalLink()
        {
            var b = new Broadcast { type = MessageType.UiManifest };
            b.elements.Add(new UIElement { id = "hp", kind = ElementKind.Stat, label = "visit https://evil.example" });
            Assert.AreEqual(PayloadValidator.Result.ExternalLink, PayloadValidator.Validate(b, Declared, Bytes(b)));
        }

        [Test] public void RejectsOversize()
        {
            var b = new Broadcast { type = MessageType.Toast, toast = new string('a', 8192) };
            Assert.AreEqual(PayloadValidator.Result.OversizePayload, PayloadValidator.Validate(b, Declared, 8200));
        }

        [Test] public void EmptyTypeRejected()
        {
            var b = new Broadcast { type = null };
            Assert.AreEqual(PayloadValidator.Result.EmptyType, PayloadValidator.Validate(b, Declared, Bytes(b)));
        }
    }
}
