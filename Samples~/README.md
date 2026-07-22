# Samples (v4 UGS)

Subclass `ExampleBase`; drop on any GameObject. Auto-initializes runtime on `Start()`.

```csharp
public class MyHud : ExampleBase
{
    protected override void OnTwitchReady(string channelId)
    {
        var b = new TwitchUIBuilder().Stat("score", "Score", 0).Build();
        _ = Send(b);
    }
}
```

**Reminder**: every element ID you Send must be in the Manifest declared in the Integration Wizard. The Cloud Code module rejects undeclared IDs server-side; the extension rejects them client-side. If you add a new ID, re-run the Manifest step → redeploy module → re-upload extension.
