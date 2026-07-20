using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._Floof.Brush;

public sealed partial class BrushSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BrushComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<BrushComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BrushComponent, BrushDoAfterEvent>(OnDoAfter);
    }

    private void OnUseInHand(Entity<BrushComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!CanBrush(ent.AsNullable(), ent))
            return;

        if (_net.IsServer) // Cannot cancel predicted audio.
            ent.Comp.AudioStream = _audio.PlayPvs(ent.Comp.MixingSound, ent)?.Entity;

        var doAfterArgs = new DoAfterArgs(EntityManager,
            args.User,
            ent.Comp.TimeToBrush,
            new BrushDoAfterEvent(),
            ent,
            ent,
            ent)
        {
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            BreakOnMove = true
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnAfterInteract(Entity<BrushComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.Target.HasValue || !args.CanReach)
            return;

        if (!CanBrush(ent.AsNullable(), args.Target.Value))
            return;

        if (_net.IsServer) // Cannot cancel predicted audio.
            ent.Comp.AudioStream = _audio.PlayPvs(ent.Comp.MixingSound, ent)?.Entity;

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.TimeToBrush, new BrushDoAfterEvent(), ent, args.Target.Value, ent);

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnDoAfter(Entity<BrushComponent> ent, ref BrushDoAfterEvent args)
    {
        ent.Comp.AudioStream = _audio.Stop(ent.Comp.AudioStream);

        if (args.Cancelled)
            return;

        if (args.Target == null)
            return;

        if (!TryBrush(ent.AsNullable(), args.Target.Value))
            return;

        _popup.PopupClient(
            Loc.GetString(ent.Comp.BrushMessage,
                ("brushed", Identity.Entity(args.Target.Value, EntityManager)),
                ("mixer", Identity.Entity(ent.Owner, EntityManager))),
            args.User,
            args.User);
        BeginBrushingEvent(ent, args.Target.Value);
        args.Handled = true;
    }

    /// <summary>
    /// Returns true if given reaction mixer is able to mix the solution inside the target entity, false otherwise.
    /// </summary>
    /// <param name="ent">The reaction mixer used to cause the reaction.</param>
    /// <param name="target">The target solution container with a <see cref="MixableSolutionComponent"/>.</param>
    public bool CanBrush(Entity<BrushComponent?> ent, EntityUid target)
    {
        if (!Resolve(ent, ref ent.Comp, false)) // The used entity needs the component to be able to mix a solution
            return false;

        // Can't mix nothing.
        if (target == null)
            return false;

        var brushAttemptEvent = new BrushingAttemptEvent(ent);
        RaiseLocalEvent(ent, ref brushAttemptEvent);
        if (brushAttemptEvent.Cancelled)
            return false;

        return true;
    }

    /// <summary>
    /// Attempts to mix the solution inside the target entity using the given reaction mixer.
    /// </summary>
    /// <param name="ent">The reaction mixer used to cause the reaction.</param>
    /// <param name="target">The target solution container with a <see cref="MixableSolutionComponent"/>.</param>
    /// <returns>If the reaction mixer was able to mix the solution. This does not necessarily mean a reaction took place.</returns>
    public bool TryBrush(Entity<BrushComponent?> ent, EntityUid target)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        var brushAttemptEvent = new BrushingAttemptEvent(ent);
        RaiseLocalEvent(ent, ref brushAttemptEvent);
        if (brushAttemptEvent.Cancelled)
            return false;

        var afterbrushAttemptEvent = new AfterBrushingEvent(ent, target);
        RaiseLocalEvent(ent, ref afterbrushAttemptEvent);

        return true;
    }

    public void BeginBrushingEvent(Entity<BrushComponent> ent, EntityUid target)
    {
        if (!Resolve(ent, ref ent.Comp!, false))
            return;
    }
}
