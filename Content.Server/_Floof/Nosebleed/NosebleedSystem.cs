using Content.Server.Body.Systems;
using Content.Shared._Floof.Util;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Floof.Nosebleed;

public sealed class NosebleedSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public static Ticker GlobalUpdateInterval = new(TimeSpan.FromMilliseconds(1000)); // stop checking everything every tick

    /// A ticker using the Floof Ticker.cs helper class to track when our next nosebleed is.
    /// This will handle getting _timing.CurTime and checking if its less than our internal.
    public Ticker NextNosebleedInterval;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Component.NosebleedComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, Component.NosebleedComponent comp, ComponentStartup args)
    {
        ScheduleNextNosebleed(new Entity<Component.NosebleedComponent>(uid, comp));
    }

    public override void Update(float frameTime)
    {
        if (!GlobalUpdateInterval.TryUpdate(_timing))
            return;

        var query = EntityQueryEnumerator<Component.NosebleedComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            var ent = new Entity<Component.NosebleedComponent>(uid, comp);

            if (!NextNosebleedInterval.TryUpdate(_timing))
                continue;

            CauseNosebleed(ent);
        }
    }

    private void ScheduleNextNosebleed(Entity<Component.NosebleedComponent> ent)
    {
        var delay = _random.Next(TimeSpan.FromSeconds(ent.Comp.MinimumDelay), TimeSpan.FromSeconds(ent.Comp.MaximumDelay));

        NextNosebleedInterval.Interval = delay;
    }

    private void CauseNosebleed(Entity<Component.NosebleedComponent> ent)
    {
        ScheduleNextNosebleed(ent);

        if (!TryComp<MobStateComponent>(ent.Owner, out var mobState))
            return;

        // are they not alive? it would be funny if we let it happen on the dead...
        if (!_mobState.IsAlive(ent.Owner, mobState))
            return;

        _popup.PopupEntity(Loc.GetString("nosebleed-message"), ent.Owner, ent.Owner, PopupType.MediumCaution);

        // bleed on the floor time (the poor janitors im sorry)
        _bloodstream.TryModifyBleedAmount(ent.Owner, ent.Comp.BleedAmount);
    }
}
