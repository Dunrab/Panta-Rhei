using Content.Shared._Floof.Util;

namespace Content.Shared._Floof.Nosebleed;

[RegisterComponent]
public sealed partial class NosebleedComponent : Component
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

    /// <summary>
    /// A ticker using the Floof Ticker.cs helper class to track when our next nosebleed is.
    /// This will handle getting _timing.CurTime and checking if its less than our internal.
    /// </summary>
    //[DataField(serverOnly: true), ViewVariables(VVAccess.ReadOnly)] //cunocmment before we commit
    [ViewVariables(VVAccess.ReadWrite)]
    public Ticker NextNosebleedInterval;
}
