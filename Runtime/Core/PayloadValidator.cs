using System.Collections.Generic;
using LiveGameLink.Protocol;

namespace LiveGameLink.Core
{
    /// Client-side mirror of the Cloud Code module's policy. Fails fast in dev. Cloud Code re-validates.
    public static class PayloadValidator
    {
        public const int MaxPayloadBytes = 4 * 1024;
        public const int MaxElements    = 64;

        public enum Result { Ok, OversizePayload, TooManyElements, UndeclaredId, BadKind, BadId, ExternalLink, EmptyType }

        public static Result Validate(Broadcast b, ICollection<string> declaredIds, int serializedByteLen)
        {
            if (b == null || string.IsNullOrEmpty(b.type)) return Result.EmptyType;
            if (serializedByteLen > MaxPayloadBytes)       return Result.OversizePayload;
            if (b.elements != null && b.elements.Count > MaxElements) return Result.TooManyElements;
            if (b.elements != null)
            {
                foreach (var e in b.elements)
                {
                    if (e == null || string.IsNullOrEmpty(e.id)) return Result.BadId;
                    if (!IsValidId(e.id))                        return Result.BadId;
                    if (System.Array.IndexOf(ElementKind.All, e.kind) < 0) return Result.BadKind;
                    if (declaredIds != null && declaredIds.Count > 0 && !declaredIds.Contains(e.id)) return Result.UndeclaredId;
                    if (ContainsExternalLink(e.label))           return Result.ExternalLink;
                }
            }
            if (ContainsExternalLink(b.toast)) return Result.ExternalLink;
            return Result.Ok;
        }

        static bool IsValidId(string id)
        {
            if (id.Length > 64) return false;
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                bool ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
                       || (c >= '0' && c <= '9') || c == '_' || c == '-';
                if (!ok) return false;
            }
            return true;
        }

        static bool ContainsExternalLink(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            string lower = s.ToLowerInvariant();
            return lower.Contains("http://") || lower.Contains("https://");
        }
    }
}
