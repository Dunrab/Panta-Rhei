using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
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

        SubscribeLocalEvent<BrushComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BrushComponent, BrushDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<BrushComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.Target.HasValue || !args.CanReach)
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

        _popup.PopupClient(
            Loc.GetString(args.Target == args.User ? ent.Comp.BrushMessage : ent.Comp.BrushMessageTarget,
                ("target", Identity.Entity(args.Target.Value, EntityManager)),
                ("user", Identity.Entity(ent.Owner, EntityManager))),
            args.User,
            args.User);
        BeginBrushingEvent(ent, args.Target.Value);
        args.Handled = true;
    }

    public void BeginBrushingEvent(Entity<BrushComponent> ent, EntityUid target)
    {
        if (!Resolve(ent, ref ent.Comp!, false))
            return;
    }
}
