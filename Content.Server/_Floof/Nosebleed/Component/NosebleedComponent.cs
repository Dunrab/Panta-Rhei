namespace Content.Server._Floof.Nosebleed.Component;

[RegisterComponent]
public sealed partial class NosebleedComponent : Robust.Shared.GameObjects.Component
{
    /// <summary>
    /// Blood that will be lost, same as the noospheric nosebleed event
    /// </summary>
    [DataField("bleedAmount"), ViewVariables(VVAccess.ReadWrite)]
    public float BleedAmount = 2.5f;

    /// <summary>
    /// Minimum delay between nosebleeds in seconds, default is 900 seconds or 15 minutes
    /// </summary>
    [DataField("minDelay"), ViewVariables(VVAccess.ReadWrite)]
    public float MinimumDelay = 900f;

    /// <summary>
    /// Maximum delay between nosebleeds in seconds, default is 3600 seconds or 60 minutes
    /// </summary>
    [DataField("maxDelay"), ViewVariables(VVAccess.ReadWrite)]
    public float MaximumDelay = 3600;
}
