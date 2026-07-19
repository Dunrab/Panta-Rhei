using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Floof.Brush;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BrushComponent : Component
{
    /// <summary>
    /// The popup message when successfully brushing someone
    /// </summary>
    [DataField, AutoNetworkedField]
    public LocId BrushMessage = "default-mixing-success";

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

[Serializable, NetSerializable]
public enum BrushTargetType
{
    Self, // targeting self
    Other // targeting somoene else
}

[ByRefEvent]
public record struct BrushuingAttemptEvent(EntityUid Brushed, bool Cancelled = false);

[ByRefEvent]
public readonly record struct AfterBrushuingEvent(EntityUid Brushed, EntityUid Brusher);

[Serializable, NetSerializable]
public sealed partial class BrushDoAfterEvent : SimpleDoAfterEvent;
