using System.Text.Json.Serialization;

namespace LiveGameLink.CloudCode;

/// Mirror of Unity-side Runtime/Protocol/Messages.cs. Keep in sync.
/// Same protocol version (3.0.0) as v3.
public static class Protocol
{
    public const string Version    = "3.0.0";
    public const int    MaxBytes   = 4 * 1024;
    public const int    MaxElems   = 64;

    public static readonly HashSet<string> Kinds = new() { "button", "progress", "stat", "toast" };

    /// Declared element manifest - set by the Integration Wizard's Manifest step.
    /// Module rejects undeclared IDs. THIS is the Twitch-policy safeguard.
    public static readonly HashSet<string> DeclaredElementIds = new()
    {
        // populated by the wizard
    };
}

public class Envelope
{
    [JsonPropertyName("protocolVersion")] public string ProtocolVersion { get; set; } = Protocol.Version;
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("ts")]   public long   Ts   { get; set; }
}

public class Broadcast : Envelope
{
    [JsonPropertyName("elements")] public List<UIElement>? Elements { get; set; }
    [JsonPropertyName("toast")]    public string?          Toast    { get; set; }
}

public class UIElement
{
    [JsonPropertyName("id")]    public string Id   { get; set; } = "";
    [JsonPropertyName("kind")]  public string Kind { get; set; } = "";
    [JsonPropertyName("label")] public string? Label { get; set; }
    [JsonPropertyName("value")] public float  Value { get; set; }
    [JsonPropertyName("min")]   public float  Min   { get; set; }
    [JsonPropertyName("max")]   public float  Max   { get; set; }
}
