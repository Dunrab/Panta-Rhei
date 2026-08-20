using Content.Shared._Floof.SelfInject.Component;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Floof.SelfInject;

public sealed class SelfInjectSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SelfInjectComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SelfInjectComponent, ActionSelfInjectEvent>(OnInject);
    }

    private void OnMapInit(Entity<SelfInjectComponent> ent, ref MapInitEvent args)
    {
        if (string.IsNullOrEmpty(ent.Comp.Action))
            return;

        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    private void OnInject(Entity<SelfInjectComponent> ent, ref ActionSelfInjectEvent args)
    {
        if (!TryComp<BloodstreamComponent>(ent, out var bloodstream))
            return;

        if (bloodstream.BloodSolution is not { } bloodSolution)
            return;

        if (!_solution.TryAddReagent(bloodSolution, ent.Comp.Reagent, ent.Comp.Amount))
            return;

        _audio.PlayPredicted(ent.Comp.InjectSound, ent, args.Performer);

        args.Handled = true;
    }
}
