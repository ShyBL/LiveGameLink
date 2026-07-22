using System.Text.RegularExpressions;
using System.Text.Json;

namespace LiveGameLink.CloudCode;

/// Server-side payload policy. Same shape as v3 (Backend~/cloudflare/src/policy.ts).
/// Rejects undeclared element IDs, bad kinds, external links, oversized payloads.
public static class Policy
{
    static readonly Regex IdPattern = new("^[a-zA-Z0-9_-]{1,64}$", RegexOptions.Compiled);
    static readonly HashSet<string> AllowedTypes = new() { "UI_MANIFEST", "STATE_PATCH", "TOAST", "CLEAR_UI" };

    public enum Code
    {
        Ok, Oversize, TooManyElements, BadKind, BadId, UndeclaredElement,
        ExternalLinkDenied, BadType, BadEnvelope
    }

    public record Result(Code Code, string? Detail = null) { public bool Ok => Code == Code.Ok; }

    public static Result Enforce(string rawJson)
    {
        if (rawJson.Length > Protocol.MaxBytes) return new(Code.Oversize);
        Broadcast? b;
        try { b = JsonSerializer.Deserialize<Broadcast>(rawJson); }
        catch { return new(Code.BadEnvelope); }
        if (b is null || string.IsNullOrEmpty(b.Type) || !AllowedTypes.Contains(b.Type))
            return new(Code.BadType);

        if (b.Elements is not null)
        {
            if (b.Elements.Count > Protocol.MaxElems) return new(Code.TooManyElements);
            foreach (var el in b.Elements)
            {
                if (string.IsNullOrEmpty(el.Id) || !IdPattern.IsMatch(el.Id)) return new(Code.BadId, el.Id);
                if (!Protocol.Kinds.Contains(el.Kind))                         return new(Code.BadKind, el.Kind);
                if (Protocol.DeclaredElementIds.Count > 0 && !Protocol.DeclaredElementIds.Contains(el.Id))
                    return new(Code.UndeclaredElement, el.Id);
                if (el.Label is not null && ContainsExternalLink(el.Label))    return new(Code.ExternalLinkDenied, el.Id);
            }
        }

        if (b.Toast is not null)
        {
            if (b.Toast.Length > 200)         return new(Code.Oversize);
            if (ContainsExternalLink(b.Toast)) return new(Code.ExternalLinkDenied);
        }

        return new(Code.Ok);
    }

    static bool ContainsExternalLink(string s) =>
        s.Contains("http://", StringComparison.OrdinalIgnoreCase)
     || s.Contains("https://", StringComparison.OrdinalIgnoreCase);
}
