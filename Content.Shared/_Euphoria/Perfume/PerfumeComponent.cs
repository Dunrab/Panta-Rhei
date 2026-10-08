using Content.Shared._Coyote.SniffAndSmell;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Euphoria.Perfume;

[RegisterComponent, NetworkedComponent]
public sealed partial class PerfumeComponent : Component
{
    /// <summary>
    /// The scent that this perfume applies to the target.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ScentPrototype> Scent;

    /// <summary>
    /// How long applying the perfume takes.
    /// </summary>
    [DataField]
    public TimeSpan UseTime = TimeSpan.FromSeconds(2);
}
