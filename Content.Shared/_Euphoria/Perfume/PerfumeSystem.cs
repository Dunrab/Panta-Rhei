using Content.Shared._Coyote.SniffAndSmell;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Euphoria.Perfume;

public sealed class PerfumeSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ScentSystem _scentSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private static readonly SoundSpecifier PerfumeSpraySound = new SoundPathSpecifier("/Audio/Effects/spray3.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<PerfumeComponent, PerfumeDoAfterEvent>(OnPerfumeDoAfter);
    }

    private void OnInteractUsing(InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<PerfumeComponent>(args.Used, out var perfume))
            return;

        var target = args.Target;

        // We have to do this otherwise the scent system runs an ensurecmop and you can make a wall or a base ball bat smell...
        if (!HasComp<ScentComponent>(target))
            return;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            args.User,
            perfume.UseTime,
            new PerfumeDoAfterEvent(),
            args.Used,
            target: args.Target,
            used: args.Used)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs, out _))
            return;

        var locKey = target == args.User ? "perfume-self-success" : "perfume-target-success";

        _popup.PopupPredicted(
            Loc.GetString(locKey,
                ("target", Identity.Entity(target, EntityManager)),
                ("user", Identity.Entity(args.User, EntityManager))),
            args.User,
            args.User);
    }

    private void OnPerfumeDoAfter(Entity<PerfumeComponent> ent, ref PerfumeDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (args.Args.Target is not { } target)
            return;

        var scentComponent = EnsureComp<ScentComponent>(target);

        scentComponent.Scents.Clear();

        _audio.PlayPredicted(PerfumeSpraySound, ent, args.Args.User);

        _scentSystem.AddScentPrototype((target, scentComponent), ent.Comp.Scent);

        args.Handled = true;
    }
}
