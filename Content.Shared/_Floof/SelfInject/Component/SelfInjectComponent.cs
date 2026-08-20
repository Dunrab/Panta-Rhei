using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Floof.SelfInject.Component;

[RegisterComponent]
public sealed partial class SelfInjectComponent : Robust.Shared.GameObjects.Component
{
    /// <summary>
    /// Action to create.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Action = string.Empty;

    [DataField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// Reagent to inject.
    /// </summary>
    [DataField(required: true)]
    public string Reagent;

    /// <summary>
    /// Amount of reagent to inject.
    /// </summary>
    [DataField]
    public FixedPoint2 Amount = 5;

    /// <summary>
    /// Sound to play when we inject.
    /// </summary>
    [DataField("injectSound")]
    public SoundSpecifier InjectSound = new SoundPathSpecifier("/Audio/Items/hypospray.ogg");
}

/// <summary>
/// Action event for injecting the chosen reagent.
/// </summary>
public sealed partial class ActionSelfInjectEvent : InstantActionEvent;
