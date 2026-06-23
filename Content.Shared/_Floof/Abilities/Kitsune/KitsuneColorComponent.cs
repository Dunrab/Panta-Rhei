using Content.Shared.Polymorph;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Floof.Abilities.Kitsune;

/// <summary>
/// This component assigns the entity with a polymorph action
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class KitsuneColorComponent : Component
{
    [DataField, AutoNetworkedField] public Color? Color;
}

[Serializable, NetSerializable]
public enum KitsuneFoxFormColorVisuals : byte
{
    Color,
    Layer
}
