using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Floof.Brush;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BrushComponent : Component
{
    /// <summary>
    /// The popup message when successfully brushing yourself
    /// </summary>
    [DataField, AutoNetworkedField]
    public LocId BrushMessage = "self-brushing-success";

    /// <summary>
    /// The popup message when successfully brushing someone
    /// </summary>
    [DataField, AutoNetworkedField]
    public LocId BrushMessageTarget = "target-brushing-success";

    /// <summary>
    /// The sound to play when brushing.
    /// </summary>
    [DataField]
    public SoundSpecifier? MixingSound;

    /// <summary>
    /// How long it takes to brush someone
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan TimeToBrush = TimeSpan.Zero;

    // Used to cancel the played sound.
    public EntityUid? AudioStream;
}
